using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class TokenService
{
    public string ExtractTokenFromResponse(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            LoginResponseWrapper wrapper = JsonUtility.FromJson<LoginResponseWrapper>(json);
            if (wrapper != null && wrapper.data != null && !string.IsNullOrEmpty(wrapper.data.token))
            {
                Debug.Log("[Login] Token trouvé dans data.token");
                return wrapper.data.token;
            }
        }
        catch { }

        try
        {
            TokenRoot root = JsonUtility.FromJson<TokenRoot>(json);
            if (root != null && !string.IsNullOrEmpty(root.token))
            {
                Debug.Log("[Login] Token trouvé dans token (racine)");
                return root.token;
            }
        }
        catch { }

        try
        {
            AccessTokenRoot acc = JsonUtility.FromJson<AccessTokenRoot>(json);
            if (acc != null && !string.IsNullOrEmpty(acc.accessToken))
            {
                Debug.Log("[Login] Token trouvé dans accessToken (racine)");
                return acc.accessToken;
            }
        }
        catch { }

        try
        {
            Match match = Regex.Match(json, "\"token\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                Debug.Log("[Login] Token trouvé via regex \"token\"");
                return match.Groups[1].Value;
            }
        }
        catch { }

        Debug.LogWarning("[Login] Impossible d'extraire un token de la réponse JSON.");
        return null;
    }

    public string BuildMockJwt(string email, int lifetimeSeconds)
    {
        string header = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
        long exp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + lifetimeSeconds;
        string payload = "{\"sub\":\"" + email + "\",\"email\":\"" + email + "\",\"exp\":" + exp + "}";

        return Base64UrlEncode(Encoding.UTF8.GetBytes(header)) + "." +
               Base64UrlEncode(Encoding.UTF8.GetBytes(payload)) + ".";
    }

    private string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public bool ValidateAndStoreToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            Jwt.I.SetToken(token, persist: true);
            Debug.Log("[Login] Jwt.I.Token = " + Jwt.I.Token);
        }

        if (!Jwt.I.IsJwtPresent())
        {
            Debug.LogError("[Login] Jeton non disponible.");
            return false;
        }

        if (Jwt.I.IsExpired(30))
        {
            Debug.LogError("[Login] Session expirée.");
            Jwt.I.Clear();
            return false;
        }

        return true;
    }
}
