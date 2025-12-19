using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using CollectionDTOs;


public class CollectionController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Templates")]
    [SerializeField] private VisualTreeAsset smallCardTemplate;
    [SerializeField] private VisualTreeAsset deckTemplate;
    [SerializeField] private VisualTreeAsset factionTemplate;

    private VisualElement root;

    // Containers
    private VisualElement cardsContainer;
    private VisualElement decksContainer;
    private VisualElement factionsContainer;
    private VisualElement cardPreview;

    // Data
    private UserCollectionDto collection;

    // State
    private Guid? selectedFactionId;
    private UserCollectionCardDto selectedCard;

    #region Unity

    private void Awake()
    {
        root = uiDocument.rootVisualElement;

        cardsContainer = root.Q<VisualElement>("Cards");
        decksContainer = root.Q<VisualElement>("MyDecks");
        factionsContainer = root.Q<VisualElement>("Filters");
        cardPreview = root.Q<VisualElement>("CardPreview");
    }

    private void Start()
    {
        FetchCollection();
        ClearCardPreview();
    }

    #endregion

    #region Data

    private void FetchCollection()
    {
        StartCoroutine(FetchCollectionFromAPI());
    }

    private IEnumerator FetchCollectionFromAPI()
    {
        // Récupérer l'URL de l'API et l'ID utilisateur
        var cfg = ConfigLoader.Load();
        string baseUrl = (cfg?.apiBaseUrl ?? "").TrimEnd('/');
        
        if (string.IsNullOrEmpty(baseUrl))
        {
            Debug.LogError("[Collection] URL API non configurée");
            yield break;
        }

        // Construire l'URL - récupérer l'ID utilisateur du JWT
        string userId = null;
        if (Jwt.I != null)
        {
            if (!Jwt.I.TryGetClaim("sub", out userId))
            {
                // fallback éventuel selon la structure du token
                Jwt.I.TryGetClaim("userId", out userId);
            }
        }
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[Collection] User ID introuvable dans le token");
            yield break;
        }

        string url = baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
            ? $"{baseUrl}/user/{userId}"
            : $"{baseUrl}/api/collection/user/{userId}";

        UnityWebRequest request = UnityWebRequest.Get(url);
        
        // Attacher le token d'authentification
        Jwt.I.AttachAuthHeader(request);
        
        request.SetRequestHeader("Content-Type", "application/json");
        
        Debug.Log($"[Collection] GET {url}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log($"[Collection] Réponse reçue: {json}");
            
            collection = JsonUtility.FromJson<UserCollectionDto>(json);
            RenderAll();
        }
        else
        {
            Debug.LogError($"[Collection] Erreur: {request.error}");
            Debug.LogError($"[Collection] Code: {request.responseCode}");
        }
    }

    #endregion

    #region Rendering

    private void RenderAll()
    {
        RenderFactions();
        RenderDecks();
        RenderCards();
    }

    private void RenderCards()
    {
        cardsContainer.Clear();

        foreach (var card in FilteredCards())
        {
            var cardElement = smallCardTemplate.Instantiate();

            BindSmallCard(cardElement, card);

            cardElement.RegisterCallback<ClickEvent>(_ =>
            {
                SelectCard(card);
            });

            cardsContainer.Add(cardElement);
        }
    }

    private void RenderDecks()
    {
        decksContainer.Clear();

        foreach (var deck in collection.Decks)
        {
            var deckElement = deckTemplate.Instantiate();

            deckElement.Q<Label>("DeckName").text = deck.DeckName;
            deckElement.Q<VisualElement>("ChampionImage").style.backgroundImage =
                new StyleBackground(LoadSprite(deck.ChampionImage));

            deckElement.RegisterCallback<ClickEvent>(_ =>
            {
                Debug.Log($"Deck sélectionné : {deck.DeckId}");
            });

            decksContainer.Add(deckElement);
        }
    }

    private void RenderFactions()
    {
        factionsContainer.Clear();

        foreach (var faction in collection.Faction)
        {
            var factionElement = factionTemplate.Instantiate();

            factionElement.Q<Label>("FactionName").text = faction.FactionName;
            factionElement.Q<VisualElement>("FactionIcon").style.backgroundImage =
                new StyleBackground(LoadSprite(faction.FactionImage));

            factionElement.RegisterCallback<ClickEvent>(_ =>
            {
                selectedFactionId = faction.FactionId;
                RenderCards();
            });

            factionsContainer.Add(factionElement);
        }
    }

    #endregion

    #region Binding

    private void BindSmallCard(VisualElement cardElement, UserCollectionCardDto data)
    {
        var card = data.Card;

        cardElement.Q<Label>("Name").text = card.Name;

        cardElement.Q<VisualElement>("Illustration").style.backgroundImage =
            new StyleBackground(LoadSprite(card.Picture));

        var aura = cardElement.Q<VisualElement>("Aura");
        aura.ClearClassList();
        aura.AddToClassList("aura-effect");

        if (data.OwnData.Count > 0)
        {
            switch (data.OwnData[0].Rarity)
            {
                case "Rare": aura.AddToClassList("rare"); break;
                case "Epic": aura.AddToClassList("epic"); break;
                case "Legendary": aura.AddToClassList("legendary"); break;
            }
        }

        var order = cardElement.Q<Label>("AttackOrder");
        int owned = 0;

        foreach (var o in data.OwnData)
            owned += o.Number;

        if (owned > 1)
        {
            order.text = owned.ToString();
            order.style.display = DisplayStyle.Flex;
        }
        else
        {
            order.style.display = DisplayStyle.None;
        }
    }

    private void SelectCard(UserCollectionCardDto card)
    {
        selectedCard = card;
        ShowCardPreview(card);
    }

    private void ShowCardPreview(UserCollectionCardDto data)
    {
        var card = data.Card;

        cardPreview.Q<Label>("Name").text = card.Name;
        cardPreview.Q<Label>("LoreDesc").text = card.Description;

        cardPreview.Q<Label>("EffectName").text = card.CardType;
        cardPreview.Q<Label>("EffectDesc").text = card.Extension;

        cardPreview.Q<VisualElement>("Illustration").style.backgroundImage =
            new StyleBackground(LoadSprite(card.Picture));

        cardPreview.Q<Label>("ATKPoints").text = card.Attack.ToString();
        cardPreview.Q<Label>("DEFPoints").text = card.Hp.ToString();

        if (card.Factions.Count > 0)
        {
            var faction = collection.Faction
                .Find(f => f.FactionId == card.Factions[0]);

            if (faction != null)
            {
                cardPreview.Q<VisualElement>("Faction").style.backgroundImage =
                    new StyleBackground(LoadSprite(faction.FactionImage));
            }
        }
    }

    private void ClearCardPreview()
    {
        cardPreview.Q<Label>("Name").text = "";
        cardPreview.Q<Label>("LoreDesc").text = "";
        cardPreview.Q<Label>("EffectName").text = "";
        cardPreview.Q<Label>("EffectDesc").text = "";
        cardPreview.Q<Label>("ATKPoints").text = "";
        cardPreview.Q<Label>("DEFPoints").text = "";
    }

    #endregion

    #region Filtering

    private IEnumerable<UserCollectionCardDto> FilteredCards()
    {
        foreach (var card in collection.Cards)
        {
            if (selectedFactionId.HasValue &&
                !card.Card.Factions.Contains(selectedFactionId.Value))
                continue;

            yield return card;
        }
    }

    #endregion

    #region Utils

    private Sprite LoadSprite(string path)
    {
        return Resources.Load<Sprite>(path);
    }

    #endregion
}
