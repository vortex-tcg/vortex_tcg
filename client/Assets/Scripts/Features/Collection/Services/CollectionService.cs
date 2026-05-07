using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.Features.Collection.Services
{
    public partial class CollectionService
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

        public IEnumerator FetchUserCollection(Action<List<UserCollectionCardDto>> onSuccess, Action<string> onError)
        {
            if (Jwt.I == null)
            {
                onError?.Invoke("[CollectionService] JWT indisponible");
                yield break;
            }

            yield return ResolveUserId(userId =>
            {
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    _resolvedUserId = userId;
                    _hasResolvedUserId = true;
                }
            }, onError);

            if (!_hasResolvedUserId || string.IsNullOrWhiteSpace(_resolvedUserId))
            {
                onError?.Invoke("[CollectionService] Impossible de recuperer l'ID utilisateur");
                yield break;
            }

            AppConfig cfg = ConfigLoader.Load();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.baseUrl))
            {
                onError?.Invoke("[CollectionService] Config API manquante");
                yield break;
            }

            string url = BuildUserCollectionUrl(cfg.baseUrl, _resolvedUserId);

            using UnityWebRequest req = UnityWebRequest.Get(url);
            Jwt.I.AttachAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"[CollectionService] Requete echouee : {req.error}");
                yield break;
            }

            string json = req.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(json))
            {
                onError?.Invoke("[CollectionService] JSON vide recu");
                yield break;
            }

            ResultDTO<UserCollectionDto> result;
            try
            {
                result = JsonConvert.DeserializeObject<ResultDTO<UserCollectionDto>>(json);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"[CollectionService] Erreur parsing JSON : {ex.Message}");
                yield break;
            }

            if (result == null)
            {
                onError?.Invoke("[CollectionService] Parsing JSON a renvoye null");
                yield break;
            }

            if (!(result.success && result.data != null))
            {
                onError?.Invoke($"[CollectionService] Reponse invalide : {result.message}");
                yield break;
            }

            onSuccess?.Invoke(result.data.Cards ?? new List<UserCollectionCardDto>());
        }

        public IEnumerator FetchUserCollectionDto(Action<UserCollectionDto> onSuccess, Action<string> onError)
        {
            if (Jwt.I == null)
            {
                onError?.Invoke("[CollectionService] JWT indisponible");
                yield break;
            }

            yield return ResolveUserId(userId =>
            {
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    _resolvedUserId = userId;
                    _hasResolvedUserId = true;
                }
            }, onError);

            if (!_hasResolvedUserId || string.IsNullOrWhiteSpace(_resolvedUserId))
            {
                onError?.Invoke("[CollectionService] Impossible de recuperer l'ID utilisateur");
                yield break;
            }

            AppConfig cfg = ConfigLoader.Load();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.baseUrl))
            {
                onError?.Invoke("[CollectionService] Config API manquante");
                yield break;
            }

            string url = BuildUserCollectionUrl(cfg.baseUrl, _resolvedUserId);

            using UnityWebRequest req = UnityWebRequest.Get(url);
            Jwt.I.AttachAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"[CollectionService] Requete echouee : {req.error}");
                yield break;
            }

            string json = req.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(json))
            {
                onError?.Invoke("[CollectionService] JSON vide recu");
                yield break;
            }

            ResultDTO<UserCollectionDto> result;
            try
            {
                result = JsonConvert.DeserializeObject<ResultDTO<UserCollectionDto>>(json);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"[CollectionService] Erreur parsing JSON : {ex.Message}");
                yield break;
            }

            if (result == null)
            {
                onError?.Invoke("[CollectionService] Parsing JSON a renvoye null");
                yield break;
            }

            if (!(result.success && result.data != null))
            {
                onError?.Invoke($"[CollectionService] Reponse invalide : {result.message}");
                yield break;
            }

            onSuccess?.Invoke(result.data);
        }
        
        private string _resolvedUserId;
        private bool _hasResolvedUserId;

        private IEnumerator ResolveUserId(Action<string> onResolved, Action<string> onError)
        {
            _resolvedUserId = null;
            _hasResolvedUserId = false;

            if (TryResolveGuidClaim(out string directUserId))
            {
                onResolved?.Invoke(directUserId);
                yield break;
            }

            if (!TryGetClaimValue("email", out string email) || string.IsNullOrWhiteSpace(email))
            {
                onError?.Invoke("[CollectionService] Aucun claim GUID ni email disponible pour retrouver l'utilisateur");
                yield break;
            }

            AppConfig cfg = ConfigLoader.Load();
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.baseUrl))
            {
                onError?.Invoke("[CollectionService] Config API manquante");
                yield break;
            }

            string usersUrl = BuildApiUrl(cfg.baseUrl, "/api/user");
            using UnityWebRequest req = UnityWebRequest.Get(usersUrl);
            Jwt.I.AttachAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"[CollectionService] Impossible de recuperer la liste des users : {req.error}");
                yield break;
            }

            ApiResult<List<UserDto>> result;
            try
            {
                result = JsonConvert.DeserializeObject<ApiResult<List<UserDto>>>(req.downloadHandler.text);
            }
            catch (Exception ex)
            {
                onError?.Invoke($"[CollectionService] Erreur parsing users : {ex.Message}");
                yield break;
            }

            UserDto matchedUser = null;
            if (result != null && result.success && result.data != null)
            {
                foreach (UserDto user in result.data)
                {
                    if (user == null) continue;

                    if (!string.IsNullOrWhiteSpace(user.email) && string.Equals(user.email, email, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedUser = user;
                        break;
                    }
                }
            }

            if (matchedUser == null || !Guid.TryParse(matchedUser.id, out _))
            {
                onError?.Invoke($"[CollectionService] Impossible de retrouver un GUID utilisateur depuis l'email '{email}'");
                yield break;
            }

            onResolved?.Invoke(matchedUser.id);
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

        private static string BuildUserCollectionUrl(string baseUrl, string userId)
        {
            string apiBase = baseUrl.TrimEnd('/');
            return apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
                ? $"{apiBase}/collection/user/{userId}"
                : $"{apiBase}/api/collection/user/{userId}";
        }

        private static string BuildApiUrl(string baseUrl, string relativePath)
        {
            string apiBase = baseUrl.TrimEnd('/');
            string normalizedRelative = relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
            return apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
                ? apiBase + normalizedRelative.Substring(4)
                : apiBase + normalizedRelative;
        }
    }
}
