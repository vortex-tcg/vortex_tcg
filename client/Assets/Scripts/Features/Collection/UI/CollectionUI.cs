using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Collection.Services;

namespace VortexTCG.Scripts.Features.Collection.UI
{
    public class CollectionUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] public VisualTreeAsset cardTemplate;

        [Header("Cost Colors")]
        [SerializeField] private Color costGreen = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color costBlue = new Color(0.2f, 0.4f, 0.9f, 1f);
        [SerializeField] private Color costOrange = new Color(1f, 0.55f, 0.1f, 1f);
        [SerializeField] private Color costRed = new Color(0.9f, 0.15f, 0.15f, 1f);
        [SerializeField] private Color costViolet = new Color(0.6f, 0.2f, 0.8f, 1f);

        private ScrollView cardsScrollView;
        private VisualElement cardsContainer;
        private VisualElement cardInformationsPreview;
        private VisualElement cardIllustration;
        private VisualElement costPointsContainer;
        private VisualElement allDecksContainer;
        private VisualElement deckButtonsContainer;
        private VisualElement selectedDeckCardsContainer;
        private Label cardNameLabel;
        private Label cardLoreLabel;
        private Label attackPointsLabel;
        private Label healthPointsLabel;
        private Label costPointsLabel;
        private CollectionService collectionService;

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogError("[CollectionUI] UIDocument non assigne");
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            cardsScrollView = root.Q<ScrollView>("CardsScrollContainer");
            cardsContainer = root.Q<VisualElement>("CardsContainer");
            cardInformationsPreview = root.Q<VisualElement>("CardInformationsPreview");
            cardIllustration = root.Q<VisualElement>("Illustration");
            costPointsContainer = root.Q<VisualElement>("CardCostPoints");
            cardNameLabel = root.Q<Label>("CardName");
            cardLoreLabel = root.Q<Label>("CardLore");
            attackPointsLabel = root.Q<Label>("ATKPoints");
            healthPointsLabel = root.Q<Label>("HPPoints");
            costPointsLabel = root.Q<Label>("CostPoints");
            allDecksContainer = root.Q<VisualElement>("AllDecks");
            deckButtonsContainer = root.Q<VisualElement>("DeckButtonsContainer");
            selectedDeckCardsContainer = root.Q<VisualElement>("SelectedCardsContainer");

            if (cardsScrollView != null)
            {
                VisualElement content = cardsScrollView.contentContainer;
                content.style.flexDirection = FlexDirection.Row;
                content.style.flexWrap = Wrap.Wrap;
                content.style.justifyContent = Justify.FlexStart;
                content.style.alignItems = Align.FlexStart;
                content.style.alignContent = Align.FlexStart;
                content.style.paddingTop = 0;
            }

            if (cardsContainer != null)
            {
                cardsContainer.style.flexDirection = FlexDirection.Row;
                cardsContainer.style.flexWrap = Wrap.Wrap;
                cardsContainer.style.justifyContent = Justify.FlexStart;
                cardsContainer.style.alignItems = Align.FlexStart;
                cardsContainer.style.alignContent = Align.FlexStart;
                cardsContainer.style.alignSelf = Align.FlexStart;
                cardsContainer.style.flexGrow = 0;
            }

            HideCardPreview();

            collectionService ??= new CollectionService();
            _deckService ??= new VortexTCG.Scripts.Features.Deck.Services.DeckService();
            StartCoroutine(LoadUserCollection());
            StartCoroutine(LoadUserCollectionAndDecks());
        }

        private VortexTCG.Scripts.Features.Deck.Services.DeckService _deckService;

        private IEnumerator LoadUserCollectionAndDecks()
        {
            List<UserCollectionDeckDto> decks = null;
            string error = null;

            yield return collectionService.FetchUserCollectionDto(
                onSuccess: dto => decks = dto?.Decks ?? new List<UserCollectionDeckDto>(),
                onError: serviceError => error = serviceError
            );

            if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogError(error);
                yield break;
            }

            DisplayDecks(decks ?? new List<UserCollectionDeckDto>());
        }

        private void DisplayDecks(List<UserCollectionDeckDto> decks)
        {
            if (deckButtonsContainer == null) return;
            deckButtonsContainer.Clear();

            if (decks == null || decks.Count == 0)
            {
                ClearSelectedDeckCards();
                return;
            }

            Button firstButton = null;
            UserCollectionDeckDto firstDeck = null;

            foreach (UserCollectionDeckDto deck in decks)
            {
                if (deck == null) continue;

                Button btn = new Button();
                btn.text = string.IsNullOrWhiteSpace(deck.DeckName) ? "Deck" : deck.DeckName;
                btn.clicked += () => SelectDeck(deck, btn);
                deckButtonsContainer.Add(btn);

                if (firstButton == null)
                {
                    firstButton = btn;
                    firstDeck = deck;
                }
            }

            if (firstDeck != null && firstButton != null)
            {
                SelectDeck(firstDeck, firstButton);
            }
        }

        private void SelectDeck(UserCollectionDeckDto deck, Button selectedButton)
        {
            HighlightSelectedDeck(selectedButton);
            StartCoroutine(LoadAndShowDeck(deck.DeckId));
        }

        private void HighlightSelectedDeck(Button selectedButton)
        {
            if (deckButtonsContainer == null) return;

            foreach (VisualElement child in deckButtonsContainer.Children())
            {
                if (child is Button button)
                    button.RemoveFromClassList("deck-button-selected");
            }

            if (selectedButton != null)
                selectedButton.AddToClassList("deck-button-selected");
        }

        private void ClearSelectedDeckCards()
        {
            if (selectedDeckCardsContainer != null)
                selectedDeckCardsContainer.Clear();
        }

        private IEnumerator LoadAndShowDeck(Guid deckId)
        {
            if (_deckService == null)
                _deckService = new VortexTCG.Scripts.Features.Deck.Services.DeckService();

            DeckDataDto deckData = null;
            string error = null;

            yield return _deckService.FetchDeckData(deckId, d => deckData = d, e => error = e);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogError(error);
                yield break;
            }

            ShowSelectedDeckCards(deckData?.Cards ?? new List<DeckCardDto>());
        }

        private void ShowSelectedDeckCards(List<DeckCardDto> cards)
        {
            if (selectedDeckCardsContainer == null) return;
            selectedDeckCardsContainer.Clear();

            foreach (DeckCardDto c in cards)
            {
                if (c == null) continue;

                for (int i = 0; i < Math.Max(c.Quantity, 1); i++)
                {
                    VisualElement cardElement = cardTemplate.CloneTree();
                    CardDto previewCard = new CardDto
                    {
                        Name = c.Name,
                        Attack = c.Attack ?? 0,
                        Hp = c.Hp ?? 0,
                        Cost = c.Cost,
                        Description = c.Description,
                        Picture = c.Picture
                    };

                    BindCardVisual(cardElement, previewCard);
                    cardElement.RegisterCallback<MouseEnterEvent>(_ => ShowCardPreview(previewCard));
                    cardElement.RegisterCallback<MouseLeaveEvent>(_ => HideCardPreview());

                    selectedDeckCardsContainer.Add(cardElement);
                }
            }
        }

        private IEnumerator LoadUserCollection()
        {
            if (cardTemplate == null)
            {
                Debug.LogError("[CollectionUI] cardTemplate non assigne");
                yield break;
            }

            if (cardsContainer == null)
            {
                Debug.LogError("[CollectionUI] cardsContainer introuvable dans l'UI");
                yield break;
            }

            List<UserCollectionCardDto> cards = null;
            string error = null;

            yield return collectionService.FetchUserCollection(
                onSuccess: fetchedCards => cards = fetchedCards,
                onError: serviceError => error = serviceError
            );

            if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogError(error);
                yield break;
            }

            DisplayCards(cards ?? new List<UserCollectionCardDto>());
        }

        private void DisplayCards(List<UserCollectionCardDto> cards)
        {
            if (cardsContainer == null) return;
            cardsContainer.Clear();

            if (cards == null || cards.Count == 0)
                return;

            foreach (UserCollectionCardDto cardData in cards)
            {
                if (cardData?.Card == null) continue;

                int ownedCopies = GetOwnedCopies(cardData);
                for (int i = 0; i < ownedCopies; i++)
                {
                    CreateCardItem(cardData);
                }
            }
        }

        private static int GetOwnedCopies(UserCollectionCardDto cardData)
        {
            if (cardData?.OwnData == null || cardData.OwnData.Count == 0)
                return 1;

            int total = 0;
            foreach (UserCollectionCardOwnDto own in cardData.OwnData)
            {
                if (own == null) continue;
                total += Math.Max(own.Number, 0);
            }

            return Math.Max(total, 1);
        }

        private void CreateCardItem(UserCollectionCardDto cardData)
        {
            VisualElement cardElement = cardTemplate.CloneTree();
            cardElement.style.flexShrink = 0;
            cardElement.style.alignSelf = Align.FlexStart;
            cardElement.style.marginBottom = 10;
            BindCardVisual(cardElement, cardData.Card);

            cardElement.RegisterCallback<MouseEnterEvent>(_ => ShowCardPreview(cardData.Card));
            cardElement.RegisterCallback<MouseLeaveEvent>(_ => HideCardPreview());

            cardsContainer.Add(cardElement);
        }

        private void BindCardVisual(VisualElement cardElement, CardDto card)
        {
            if (cardElement == null || card == null) return;

            Label nameLabel = cardElement.Q<Label>("Name");
            if (nameLabel != null) nameLabel.text = card.Name;

            Label atkLabel = cardElement.Q<Label>("ATK");
            if (atkLabel != null) atkLabel.text = card.Attack.ToString();

            Label defLabel = cardElement.Q<Label>("DEF");
            if (defLabel != null) defLabel.text = card.Hp.ToString();

            Label costLabel = cardElement.Q<Label>("Cost");
            if (costLabel != null) costLabel.text = card.Cost.ToString();

            VisualElement costCircle = cardElement.Q<VisualElement>("CostCircle");
            if (costCircle == null) return;

            int clamped = Mathf.Clamp(card.Cost, 0, 10);
            Color circleColor = clamped switch
            {
                0 or 1 or 2 => costGreen,
                3 or 4 => costBlue,
                5 or 6 => costOrange,
                7 or 8 => costRed,
                _ => costViolet
            };

            costCircle.style.unityBackgroundImageTintColor = new StyleColor(circleColor);
        }

        private void ShowCardPreview(CardDto card)
        {
            if (cardInformationsPreview == null || card == null) return;

            if (cardNameLabel != null)
                cardNameLabel.text = card.Name;

            if (cardLoreLabel != null)
                cardLoreLabel.text = card.Description;

            if (attackPointsLabel != null)
                attackPointsLabel.text = card.Attack.ToString();

            if (healthPointsLabel != null)
                healthPointsLabel.text = card.Hp.ToString();

            if (costPointsLabel != null)
            {
                costPointsLabel.text = card.Cost.ToString();
                UpdateCostColor(card.Cost);
            }

            if (cardIllustration != null)
            {
                Texture2D illustration = ResolveCardTexture(card.Picture);
                if (illustration != null)
                    cardIllustration.style.backgroundImage = new StyleBackground(illustration);
            }

            cardInformationsPreview.style.display = DisplayStyle.Flex;
        }

        private void HideCardPreview()
        {
            if (cardInformationsPreview != null)
                cardInformationsPreview.style.display = DisplayStyle.None;
        }

        private void UpdateCostColor(int cost)
        {
            if (costPointsContainer == null)
                return;

            int clampedCost = Mathf.Clamp(cost, 0, 10);

            Color target = clampedCost switch
            {
                0 or 1 or 2 => costGreen,
                3 or 4 => costBlue,
                5 or 6 => costOrange,
                7 or 8 => costRed,
                _ => costViolet
            };

            costPointsContainer.style.backgroundColor = target;
        }

        private static Texture2D ResolveCardTexture(string picture)
        {
            if (string.IsNullOrWhiteSpace(picture))
                return null;

            Texture2D texture = Resources.Load<Texture2D>(picture);
            if (texture != null)
                return texture;

            return Resources.Load<Texture2D>(picture.TrimStart('/'));
        }
    }
}
