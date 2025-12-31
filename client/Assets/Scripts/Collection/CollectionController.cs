using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.CollectionScene
{
    public class CollectionController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset cardTemplate;

        private VisualElement root;
        private VisualElement cardsContainer;
        private VisualElement cardPreview;

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            root = uiDocument.rootVisualElement;

            cardsContainer = root.Q<VisualElement>("CardsContainer");
            cardPreview = root.Q<VisualElement>("CardPreview");

            if (cardPreview != null)
            {
                cardPreview.style.display = DisplayStyle.Flex;
            }

            StartCoroutine(LoadUserCollection());
        }

        private IEnumerator LoadUserCollection()
        {
            if (!Jwt.I.TryGetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out string userId))
            {
                Debug.LogError("[CollectionController] Impossible de récupérer l’ID utilisateur");
                yield break;
            }

            AppConfig cfg = ConfigLoader.Load();
            if (cfg == null || string.IsNullOrEmpty(cfg.baseUrl))
            {
                Debug.LogError("[CollectionController] Config API manquante");
                yield break;
            }

            string apiBase = cfg.baseUrl.TrimEnd('/');
            string url = apiBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
                ? $"{apiBase}/collection/user/{userId}"
                : $"{apiBase}/api/collection/user/{userId}";

            using UnityWebRequest req = UnityWebRequest.Get(url);
            Jwt.I.AttachAuthHeader(req);

            yield return req.SendWebRequest();

            string json = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[CollectionController] Requête échouée : {req.error}");
                yield break;
            }

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[CollectionController] JSON vide reçu !");
                yield break;
            }

            ResultDTO<UserCollectionDto> result;
            try
            {
                result = JsonConvert.DeserializeObject<ResultDTO<UserCollectionDto>>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CollectionController] Erreur parsing JSON : {ex.Message}");
                yield break;
            }

            if (result == null)
            {
                Debug.LogError("[CollectionController] Parsing JSON a renvoyé null !");
                yield break;
            }

            if (!(result.success && result.data != null))
            {
                Debug.LogError($"[CollectionController] Réponse invalide : {result.message}");
                yield break;
            }

            if (result.data.Cards == null)
            {
                Debug.LogError("[CollectionController] Pas de liste de cartes dans la réponse (Cards null)");
                yield break;
            }

            if (cardTemplate == null)
            {
                Debug.LogError("[CollectionController] cardTemplate non assigné");
                yield break;
            }

            if (cardsContainer == null)
            {
                Debug.LogError("[CollectionController] cardsContainer introuvable dans l'UI");
                yield break;
            }

            DisplayCards(result.data.Cards);
        }

        private void DisplayCards(List<UserCollectionCardDto> cards)
        {
            if (cardsContainer == null) return;
            cardsContainer.Clear();

            if (cards == null || cards.Count == 0)
            {
                return;
            }

            foreach (UserCollectionCardDto cardData in cards)
            {
                if (cardData?.Card == null) continue;
                CreateCardItem(cardData);
            }
        }

        private void CreateCardItem(UserCollectionCardDto cardData)
        {
            VisualElement cardElement = cardTemplate.CloneTree();
            Label nameLabel = cardElement.Q<Label>("Name");
            if (nameLabel != null) nameLabel.text = cardData.Card.Name;

            Label atkLabel = cardElement.Q<Label>("ATK");
            if (atkLabel != null) atkLabel.text = cardData.Card.Attack.ToString();

            Label defLabel = cardElement.Q<Label>("DEF");
            if (defLabel != null) defLabel.text = cardData.Card.Hp.ToString();

            Label costLabel = cardElement.Q<Label>("COST");
            if (costLabel != null) costLabel.text = cardData.Card.Cost.ToString();

            VisualElement illustration = cardElement.Q<VisualElement>("Illustration");

            cardElement.RegisterCallback<MouseEnterEvent>(_ => ShowCardPreview(cardData.Card));
            cardElement.RegisterCallback<MouseLeaveEvent>(_ => HideCardPreview());

            cardsContainer.Add(cardElement);
        }

        private void ShowCardPreview(CardDto card)
        {
            if (cardPreview == null) return;

            Label nameLabel = cardPreview.Q<Label>("Name");
            if (nameLabel != null) nameLabel.text = card.Name;

            Label atkLabel = cardPreview.Q<Label>("ATK");
            if (atkLabel != null) atkLabel.text = card.Attack.ToString();

            Label defLabel = cardPreview.Q<Label>("DEF");
            if (defLabel != null) defLabel.text = card.Hp.ToString();

            Label costLabel = cardPreview.Q<Label>("COST");
            if (costLabel != null) costLabel.text = card.Cost.ToString();

            Label loreLabel = cardPreview.Q<Label>("Lore");
            if (loreLabel != null) loreLabel.text = card.Description;

            Label effectDescLabel = cardPreview.Q<Label>("EffectDesc");
            if (effectDescLabel != null) effectDescLabel.text = card.Description;

            VisualElement illustration = cardPreview.Q<VisualElement>("Illustration");

            cardPreview.style.display = DisplayStyle.Flex;
        }

        private void HideCardPreview()
        {
            if (cardPreview != null)
                cardPreview.style.display = DisplayStyle.None;
        }
    }
}
