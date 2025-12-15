using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "JwtStore", menuName = "Vortex/Auth/JWT Store")]
public class JwtStore : ScriptableObject
{
    [Header("User session (read-only)")]
    [SerializeField] private string userId;
    [SerializeField] private string username;
    [SerializeField] private string role;
    [SerializeField] private string token;

    [Header("Persistence keys")]
    [SerializeField] private string playerPrefsKeyToken = "jwt_token";
    [SerializeField] private string playerPrefsKeyUserId = "jwt_userId";
    [SerializeField] private string playerPrefsKeyUsername = "jwt_username";
    [SerializeField] private string playerPrefsKeyRole = "jwt_role";

    private Dictionary<string, object> claims;

    public string Token => token;
    public string UserId => userId;
    public string Username => username;
    public string Role => role;

    public bool IsJwtPresent() => !string.IsNullOrEmpty(token);
    public bool HasClaim(string key) => claims != null && claims.ContainsKey(key);

    public bool IsExpired(int leewaySeconds = 0)
    {
        if (!TryGetClaim("exp", out var expStr)) return false;
        if (!long.TryParse(expStr, out var expUnix)) return false;
        var expUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
        return DateTime.UtcNow >= expUtc.AddSeconds(leewaySeconds);
    }

    public double? SecondsToExpiry()
    {
        if (!TryGetClaim("exp", out var expStr)) return null;
        if (!long.TryParse(expStr, out var expUnix)) return null;
        var expUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
        return (expUtc - DateTime.UtcNow).TotalSeconds;
    }
    public void SetSession(string id, string username, string role, string jwt, bool persist = true)
    {
        userId = id;
        this.username = username;
        this.role = role;

        token = jwt;
        ParseClaims();

        if (persist)
        {
            PlayerPrefs.SetString(playerPrefsKeyToken, token ?? "");
            PlayerPrefs.SetString(playerPrefsKeyUserId, userId ?? "");
            PlayerPrefs.SetString(playerPrefsKeyUsername, this.username ?? "");
            PlayerPrefs.SetString(playerPrefsKeyRole, this.role ?? "");
            PlayerPrefs.Save();
        }
    }

    public void Clear(bool removePersist = true)
    {
        token = null;
        claims = null;
        userId = null;
        username = null;
        role = null;

        if (removePersist)
        {
            PlayerPrefs.DeleteKey(playerPrefsKeyToken);
            PlayerPrefs.DeleteKey(playerPrefsKeyUserId);
            PlayerPrefs.DeleteKey(playerPrefsKeyUsername);
            PlayerPrefs.DeleteKey(playerPrefsKeyRole);
            PlayerPrefs.Save();
        }
    }

    public bool LoadFromPrefs()
    {
        token = PlayerPrefs.GetString(playerPrefsKeyToken, "");
        userId = PlayerPrefs.GetString(playerPrefsKeyUserId, "");
        username = PlayerPrefs.GetString(playerPrefsKeyUsername, "");
        role = PlayerPrefs.GetString(playerPrefsKeyRole, "");

        ParseClaims();
        return !string.IsNullOrEmpty(token);
    }

    public Dictionary<string, object> GetAllClaims()
        => claims != null ? new Dictionary<string, object>(claims) : new Dictionary<string, object>();

    public bool TryGetClaim(string key, out string value)
    {
        value = null;
        if (claims == null || !claims.ContainsKey(key)) return false;
        var v = claims[key];
        value = v?.ToString();
        return true;
    }

    public void AttachAuthHeader(UnityWebRequest req)
    {
        if (IsJwtPresent())
            req.SetRequestHeader("Authorization", "Bearer " + token);
    }

    private void ParseClaims()
    {
        claims = null;
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return;

            string payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            claims = MiniJson.Deserialize(payloadJson) as Dictionary<string, object>;
            if (claims != null)
            {
                foreach (var k in new List<string>(claims.Keys))
                {
                    if (claims[k] is List<object> list)
                        claims[k] = string.Join(",", list);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("JWT Parse error: " + e.Message);
            claims = null;
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        string output = input.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 0: break;
            case 2: output += "=="; break;
            case 3: output += "="; break;
            default: throw new FormatException("Illegal base64url string!");
        }
        return Convert.FromBase64String(output);
    }

    private static class MiniJson
    {
        public static object Deserialize(string json) => Parser.Parse(json);

        private class Parser
        {
            private readonly string json; private int index;
            private Parser(string json) { this.json = json; index = 0; }
            public static object Parse(string json) => new Parser(json).ParseValue();

            private Dictionary<string, object> ParseObject()
            {
                var dict = new Dictionary<string, object>(); 
                NextChar(); 
                while (true)
                {
                    SkipWs();
                    if (PeekChar() == '}') { NextChar(); break; }
                    string key = ParseString();
                    SkipWs(); NextChar(); 
                    var val = ParseValue();
                    dict[key] = val;
                    SkipWs();
                    var c = PeekChar();
                    if (c == ',') { NextChar(); continue; }
                    if (c == '}') { NextChar(); break; }
                }
                return dict;
            }

            private List<object> ParseArray()
            {
                var list = new List<object>(); NextChar(); 
                while (true)
                {
                    SkipWs();
                    if (PeekChar() == ']') { NextChar(); break; }
                    list.Add(ParseValue());
                    SkipWs();
                    var c = PeekChar();
                    if (c == ',') { NextChar(); continue; }
                    if (c == ']') { NextChar(); break; }
                }
                return list;
            }

            private object ParseValue()
            {
                SkipWs();
                char c = PeekChar();
                if (c == '"') return ParseString();
                if (c == '{') return ParseObject();
                if (c == '[') return ParseArray();
                if (char.IsDigit(c) || c == '-' ) return ParseNumber();
                if (Match("true")) return true;
                if (Match("false")) return false;
                if (Match("null")) return null;
                return null;
            }

            private string ParseString()
            {
                var sb = new StringBuilder(); NextChar(); 
                while (true)
                {
                    char c = NextChar();
                    if (c == '"') break;
                    if (c == '\\')
                    {
                        c = NextChar();
                        if (c == '"' || c == '\\' || c == '/') sb.Append(c);
                        else if (c == 'b') sb.Append('\b');
                        else if (c == 'f') sb.Append('\f');
                        else if (c == 'n') sb.Append('\n');
                        else if (c == 'r') sb.Append('\r');
                        else if (c == 't') sb.Append('\t');
                        else if (c == 'u')
                        {
                            string hex = json.Substring(index, 4); index += 4;
                            sb.Append((char)Convert.ToInt32(hex, 16));
                        }
                    }
                    else sb.Append(c);
                }
                return sb.ToString();
            }

            private object ParseNumber()
            {
                int start = index;
                while (index < json.Length && "0123456789+-.eE".IndexOf(json[index]) != -1) index++;
                string num = json.Substring(start, index - start);
                if (num.IndexOf('.') != -1 || num.IndexOf('e') != -1 || num.IndexOf('E') != -1)
                { if (double.TryParse(num, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d; }
                if (long.TryParse(num, out var l)) return l.ToString();
                return num;
            }

            private void SkipWs(){ while (index < json.Length && char.IsWhiteSpace(json[index])) index++; }
            private bool Match(string s){ SkipWs(); if (index + s.Length > json.Length) return false; if (json.Substring(index, s.Length) == s) { index += s.Length; return true; } return false; }
            private char PeekChar(){ return index < json.Length ? json[index] : '\0'; }
            private char NextChar(){ return index < json.Length ? json[index++] : '\0'; }
        }
    }
}
