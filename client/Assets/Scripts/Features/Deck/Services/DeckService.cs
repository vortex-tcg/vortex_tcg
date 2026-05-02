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
        [Serializable]
        private class ApiResult<T>
        {
            public bool success;
            public int statusCode;
            public string message;
            public T data;
        }

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
    }
}
