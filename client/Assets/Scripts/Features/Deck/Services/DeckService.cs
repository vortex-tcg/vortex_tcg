using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.Features.Deck.Services
{
    public class DeckService
    {
        private const string NameIdentifierClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private static readonly string[] FallbackUserIdClaims = { "id", "userId", "sub" };

        [Serializable]
        private class ApiResult<T>
        {
            public bool success;
            public int statusCode;
            public string message;
            public T data;
        }

        [Serializable]
        private class UserDto
        {
            public string id;
            public string username;
            public string email;
        }

        [Serializable]
        private class CreateDeckPayload
        {
            public string Label;
            public Guid UserId;
            public Guid ChampionId;
            public Guid FactionId;
        }

        private string _resolvedUserId;
        private Guid _resolvedUserGuid;
        private bool _hasResolvedUserId;

        public IEnumerator FetchDeckData(Guid deckId, Action<DeckDataDto> onSuccess, Action<string> onError)
        {
            AppConfig cfg = ConfigLoader.Load();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.baseUrl))
            {
                onError?.Invoke("[DeckService] Config API manquante");
                yield break;
            }

            string apiBase = cfg.baseUrl.TrimEnd('/');
            string url = apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
                ? $"{apiBase}/deck/getDeckData/{deckId}"
                : $"{apiBase}/api/deck/getDeckData/{deckId}";

            using UnityWebRequest req = UnityWebRequest.Get(url);
            if (Jwt.I != null)
                Jwt.I.AttachAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"[DeckService] Requete echouee : {req.error}");
                yield break;
            }

            string json = req.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(json))
            {
                onError?.Invoke("[DeckService] JSON vide recu");
                yield break;
            }

            ApiResult<DeckDataDto> result;
            try
            {
                result = JsonConvert.DeserializeObject<ApiResult<DeckDataDto>>(json);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"[DeckService] Erreur parsing JSON : {ex.Message}");
                yield break;
            }

            if (result == null)
            {
                onError?.Invoke("[DeckService] Parsing JSON a renvoye null");
                yield break;
            }

            if (!(result.success && result.data != null))
            {
                onError?.Invoke($"[DeckService] Reponse invalide : {result.message}");
                yield break;
            }

            onSuccess?.Invoke(result.data);
        }

        public IEnumerator UpdateDeckAsync(Guid deckId, UpdateDeckDto deckUpdate, Action onSuccess, Action<string> onError)
        {
            if (deckUpdate == null)
            {
                onError?.Invoke("[DeckService] Payload de mise a jour manquant");
                yield break;
            }

            AppConfig cfg = ConfigLoader.Load();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.baseUrl))
            {
                onError?.Invoke("[DeckService] Config API manquante");
                yield break;
            }

            string apiBase = cfg.baseUrl.TrimEnd('/');
            string url = apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
                ? $"{apiBase}/deck/{deckId}"
                : $"{apiBase}/api/deck/{deckId}";

            string payload = JsonConvert.SerializeObject(deckUpdate);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);

            using UnityWebRequest req = new UnityWebRequest(url, "PUT");
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            if (Jwt.I != null)
                Jwt.I.AttachAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"[DeckService] Mise a jour du deck echouee : {req.error}");
                yield break;
            }

            onSuccess?.Invoke();
        }

        public IEnumerator CreateDeckAsync(string deckName, Guid championId, Guid factionId, Action onSuccess, Action<string> onError)
        {
            if (championId == Guid.Empty)
            {
                onError?.Invoke("[DeckService] ChampionId requis");
                yield break;
            }

            if (factionId == Guid.Empty)
            {
                onError?.Invoke("[DeckService] FactionId requis");
                yield break;
            }

            yield return ResolveUserId(onError);

            if (!_hasResolvedUserId || _resolvedUserGuid == Guid.Empty)
            {
                onError?.Invoke("[DeckService] Impossible de recuperer l'ID utilisateur");
                yield break;
            }

            Guid userId = _resolvedUserGuid;

            AppConfig cfg = ConfigLoader.Load();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.baseUrl))
            {
                onError?.Invoke("[DeckService] Config API manquante");
                yield break;
            }

            string apiBase = cfg.baseUrl.TrimEnd('/');
            string url = apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
                ? $"{apiBase}/deck"
                : $"{apiBase}/api/deck";

            CreateDeckPayload payload = new CreateDeckPayload
            {
                Label = string.IsNullOrWhiteSpace(deckName) ? "Deck" : deckName.Trim(),
                UserId = userId,
                ChampionId = championId,
                FactionId = factionId
            };

            string json = JsonConvert.SerializeObject(payload);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            using UnityWebRequest req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            if (Jwt.I != null)
                Jwt.I.AttachAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"[DeckService] Creation du deck echouee : {req.error}");
                yield break;
            }

            onSuccess?.Invoke();
        }

        public IEnumerator DeleteDeckAsync(Guid deckId, Action onSuccess, Action<string> onError)
        {
            AppConfig cfg = ConfigLoader.Load();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.baseUrl))
            {
                onError?.Invoke("[DeckService] Config API manquante");
                yield break;
            }

            string apiBase = cfg.baseUrl.TrimEnd('/');
            string url = apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
                ? $"{apiBase}/deck/{deckId}"
                : $"{apiBase}/api/deck/{deckId}";

            using UnityWebRequest req = UnityWebRequest.Delete(url);

            if (Jwt.I != null)
                Jwt.I.AttachAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"[DeckService] Suppression du deck echouee : {req.error}");
                yield break;
            }

            onSuccess?.Invoke();
        }

        private IEnumerator ResolveUserId(Action<string> onError)
        {
            _resolvedUserId = null;
            _resolvedUserGuid = Guid.Empty;
            _hasResolvedUserId = false;

            if (TryResolveGuidClaim(out string directUserId))
            {
                _resolvedUserId = directUserId;
                _resolvedUserGuid = Guid.Parse(directUserId);
                _hasResolvedUserId = true;
                yield break;
            }

            if (!TryGetClaimValue("email", out string email) || string.IsNullOrWhiteSpace(email))
            {
                onError?.Invoke("[DeckService] Aucun claim GUID ni email disponible pour retrouver l'utilisateur");
                yield break;
            }

            AppConfig cfg = ConfigLoader.Load();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.baseUrl))
            {
                onError?.Invoke("[DeckService] Config API manquante");
                yield break;
            }

            string apiBase = cfg.baseUrl.TrimEnd('/');
            string usersUrl = apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
                ? $"{apiBase}/user"
                : $"{apiBase}/api/user";

            using UnityWebRequest req = UnityWebRequest.Get(usersUrl);
            if (Jwt.I != null)
                Jwt.I.AttachAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"[DeckService] Impossible de recuperer la liste des users : {req.error}");
                yield break;
            }

            ApiResult<List<UserDto>> result;
            try
            {
                result = JsonConvert.DeserializeObject<ApiResult<List<UserDto>>>(req.downloadHandler.text);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"[DeckService] Erreur parsing users : {ex.Message}");
                yield break;
            }

            UserDto matchedUser = null;
            if (result != null && result.success && result.data != null)
            {
                foreach (UserDto user in result.data)
                {
                    if (user == null)
                        continue;

                    if (!string.IsNullOrWhiteSpace(user.email) && string.Equals(user.email, email, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedUser = user;
                        break;
                    }
                }
            }

            if (matchedUser == null || !Guid.TryParse(matchedUser.id, out Guid matchedGuid))
            {
                onError?.Invoke($"[DeckService] Impossible de retrouver un GUID utilisateur depuis l'email '{email}'");
                yield break;
            }

            _resolvedUserId = matchedUser.id;
            _resolvedUserGuid = matchedGuid;
            _hasResolvedUserId = true;
        }

        private bool TryResolveGuidClaim(out string userId)
        {
            if (TryGetClaimValue(NameIdentifierClaim, out userId) && Guid.TryParse(userId, out _))
                return true;

            for (int i = 0; i < FallbackUserIdClaims.Length; i++)
            {
                if (TryGetClaimValue(FallbackUserIdClaims[i], out userId) && Guid.TryParse(userId, out _))
                    return true;
            }

            userId = null;
            return false;
        }

        private bool TryGetClaimValue(string claimName, out string value)
        {
            value = null;
            return Jwt.I != null && Jwt.I.TryGetClaim(claimName, out value) && !string.IsNullOrWhiteSpace(value);
        }
    }
}
