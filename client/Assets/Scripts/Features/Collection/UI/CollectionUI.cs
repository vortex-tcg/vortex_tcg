using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private VisualElement rootVisualElement;
        private VisualElement cardInformationsPreview;
        private VisualElement cardIllustration;
        private VisualElement costPointsContainer;
        private VisualElement deckDropZone;
        private VisualElement allDecksContainer;
        private VisualElement deckButtonsContainer;
        private VisualElement deckNameContainer;
        private Label deckNameLabel;
        private Button editDeckNameButton;
        private TextField deckNameTextField;
        private Button selectedDeckButton;
        private VisualElement selectedDeckCardsContainer;
        private Label cardNameLabel;
        private Label cardLoreLabel;
        private Label attackPointsLabel;
        private Label healthPointsLabel;
        private Label costPointsLabel;
        private CollectionService collectionService;
        private VortexTCG.Scripts.Features.Deck.Services.DeckService deckService;

        private Guid currentDeckId = Guid.Empty;
        private string currentDeckName = "";
        private Guid currentChampionId = Guid.Empty;
        private Guid currentFactionId = Guid.Empty;
        private List<DeckCardDto> currentDeckCards = new List<DeckCardDto>();

        private enum DragSourceType
        {
            None,
            Collection,
            Deck
        }

        private DragSourceType activeDragSource = DragSourceType.None;
        private CardDto draggedPreviewCard;
        private UserCollectionCardDto draggedCollectionCard;
        private DeckCardDto draggedDeckCard;
        private VisualElement draggedCardSource;
        private VisualElement draggedCardGhost;
        private Vector2 dragStartPosition;
        private int activeDragPointerId = -1;
        private bool isDraggingCard;
        private bool isDeckDropZoneHighlighted;

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogError("[CollectionUI] UIDocument non assigne");
                return;
            }

            rootVisualElement = uiDocument.rootVisualElement;
            cardsScrollView = rootVisualElement.Q<ScrollView>("CardsScrollContainer");
            cardsContainer = rootVisualElement.Q<VisualElement>("CardsContainer");
            cardInformationsPreview = rootVisualElement.Q<VisualElement>("CardInformationsPreview");
            cardIllustration = rootVisualElement.Q<VisualElement>("Illustration");
            costPointsContainer = rootVisualElement.Q<VisualElement>("CardCostPoints");
            cardNameLabel = rootVisualElement.Q<Label>("CardName");
            cardLoreLabel = rootVisualElement.Q<Label>("CardLore");
            attackPointsLabel = rootVisualElement.Q<Label>("ATKPoints");
            healthPointsLabel = rootVisualElement.Q<Label>("HPPoints");
            costPointsLabel = rootVisualElement.Q<Label>("CostPoints");
            allDecksContainer = rootVisualElement.Q<VisualElement>("AllDecks");
            deckButtonsContainer = rootVisualElement.Q<VisualElement>("DeckButtonsContainer");
            deckNameContainer = rootVisualElement.Q<VisualElement>("DeckNameContainer");
            deckNameLabel = rootVisualElement.Q<Label>("DeckName");
            editDeckNameButton = rootVisualElement.Q<Button>("EditDeckName");
            selectedDeckCardsContainer = rootVisualElement.Q<VisualElement>("SelectedCardsContainer");
            deckDropZone = rootVisualElement.Q<VisualElement>("DragAndDropZone");

            InitializeDeckNameEditor();

            if (deckDropZone == null)
                deckDropZone = selectedDeckCardsContainer;

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
            deckService ??= new VortexTCG.Scripts.Features.Deck.Services.DeckService();
            StartCoroutine(LoadUserCollection());
            StartCoroutine(LoadUserCollectionAndDecks());
        }

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

            Dictionary<Guid, Button> existingButtons = deckButtonsContainer
                .Children()
                .OfType<Button>()
                .Where(button => button.userData is Guid)
                .ToDictionary(button => (Guid)button.userData, button => button);

            bool hasExistingButtons = existingButtons.Count > 0;
            selectedDeckButton = hasExistingButtons ? selectedDeckButton : null;

            if (decks == null || decks.Count == 0)
            {
                if (!hasExistingButtons)
                    ResetCurrentDeckState();

                return;
            }

            Button firstCreatedButton = null;
            UserCollectionDeckDto firstDeck = null;

            foreach (UserCollectionDeckDto deck in decks)
            {
                if (deck == null) continue;

                if (existingButtons.TryGetValue(deck.DeckId, out Button existingButton))
                {
                    existingButton.text = string.IsNullOrWhiteSpace(deck.DeckName) ? "Deck" : deck.DeckName;
                    if (firstCreatedButton == null)
                    {
                        firstCreatedButton = existingButton;
                        firstDeck = deck;
                    }

                    continue;
                }

                Button btn = new Button();
                btn.userData = deck.DeckId;
                btn.name = $"DeckButton_{deck.DeckId}";
                btn.text = string.IsNullOrWhiteSpace(deck.DeckName) ? "Deck" : deck.DeckName;
                btn.AddToClassList("vortexButton");
                btn.style.borderTopWidth = 0;
                btn.style.borderRightWidth = 0;
                btn.style.borderBottomWidth = 0;
                btn.style.borderLeftWidth = 0;
                btn.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));
                btn.clicked += () => SelectDeck(deck, btn);
                deckButtonsContainer.Add(btn);

                if (firstCreatedButton == null)
                {
                    firstCreatedButton = btn;
                    firstDeck = deck;
                }
            }

            if (!hasExistingButtons && firstDeck != null && firstCreatedButton != null)
            {
                SelectDeck(firstDeck, firstCreatedButton);
            }
        }

        private void SelectDeck(UserCollectionDeckDto deck, Button selectedButton)
        {
            CommitDeckNameEditIfNeeded();
            HighlightSelectedDeck(selectedButton);
            selectedDeckButton = selectedButton;
            currentChampionId = deck.ChampionId;
            currentFactionId = deck.FactionId;
            SetCurrentDeckName(deck.DeckName);
            StartCoroutine(LoadAndShowDeck(deck.DeckId, deck.DeckName));
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

        private void ResetCurrentDeckState()
        {
            currentDeckId = Guid.Empty;
            SetCurrentDeckName("");
            currentChampionId = Guid.Empty;
            currentFactionId = Guid.Empty;
            currentDeckCards.Clear();
            ClearSelectedDeckCards();
            selectedDeckButton = null;
            SetDeckNameEditMode(false);
        }

        private IEnumerator LoadAndShowDeck(Guid deckId, string deckName)
        {
            if (deckService == null)
                deckService = new VortexTCG.Scripts.Features.Deck.Services.DeckService();

            currentDeckId = deckId;
            SetCurrentDeckName(deckName);

            DeckDataDto deckData = null;
            string error = null;

            yield return deckService.FetchDeckData(deckId, d => deckData = d, e => error = e);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogError(error);
                yield break;
            }

            currentChampionId = deckData?.Champion?.ChampionID ?? Guid.Empty;
            currentFactionId = deckData?.Champion?.FactionId ?? Guid.Empty;
            currentDeckCards = deckData?.Cards != null ? new List<DeckCardDto>(deckData.Cards) : new List<DeckCardDto>();

            ShowSelectedDeckCards(currentDeckCards);
        }

        private void ShowSelectedDeckCards(List<DeckCardDto> cards)
        {
            if (selectedDeckCardsContainer == null)
                return;

            selectedDeckCardsContainer.Clear();

            foreach (DeckCardDto c in cards)
            {
                if (c == null) continue;

                for (int i = 0; i < Math.Max(c.Quantity, 1); i++)
                {
                    VisualElement cardElement = cardTemplate.CloneTree();
                    CardDto previewCard = CreateDeckPreviewCard(c);

                    BindCardVisual(cardElement, previewCard);
                    cardElement.RegisterCallback<MouseEnterEvent>(_ => ShowCardPreview(previewCard));
                    cardElement.RegisterCallback<MouseLeaveEvent>(_ => HideCardPreview());
                    RegisterDeckCardDrag(cardElement, c, previewCard);

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
            RegisterCollectionCardDrag(cardElement, cardData);

            cardElement.RegisterCallback<MouseEnterEvent>(_ => ShowCardPreview(cardData.Card));
            cardElement.RegisterCallback<MouseLeaveEvent>(_ => HideCardPreview());

            cardsContainer.Add(cardElement);
        }

        private void RegisterCollectionCardDrag(VisualElement cardElement, UserCollectionCardDto cardData)
        {
            if (cardElement == null || cardData?.Card == null)
                return;

            cardElement.RegisterCallback<PointerDownEvent>(evt => BeginCardDrag(evt, DragSourceType.Collection, cardElement, cardData.Card, cardData.CollectionCardId, null), TrickleDown.TrickleDown);
            cardElement.RegisterCallback<PointerMoveEvent>(OnCardDragMove, TrickleDown.TrickleDown);
            cardElement.RegisterCallback<PointerUpEvent>(OnCardDragEnd, TrickleDown.TrickleDown);
            cardElement.RegisterCallback<PointerCancelEvent>(OnCardDragCancel, TrickleDown.TrickleDown);
        }

        private void RegisterDeckCardDrag(VisualElement cardElement, DeckCardDto deckCard, CardDto previewCard)
        {
            if (cardElement == null || deckCard == null || previewCard == null)
                return;

            cardElement.RegisterCallback<PointerDownEvent>(evt => BeginCardDrag(evt, DragSourceType.Deck, cardElement, previewCard, deckCard.CollectionCardId, deckCard), TrickleDown.TrickleDown);
            cardElement.RegisterCallback<PointerMoveEvent>(OnCardDragMove, TrickleDown.TrickleDown);
            cardElement.RegisterCallback<PointerUpEvent>(OnCardDragEnd, TrickleDown.TrickleDown);
            cardElement.RegisterCallback<PointerCancelEvent>(OnCardDragCancel, TrickleDown.TrickleDown);
        }

        private void BeginCardDrag(PointerDownEvent evt, DragSourceType sourceType, VisualElement sourceElement, CardDto previewCard, Guid collectionCardId, DeckCardDto deckCard)
        {
            if (evt == null || sourceElement == null || previewCard == null || evt.button != 0)
                return;

            CancelCardDrag(false);

            activeDragSource = sourceType;
            draggedPreviewCard = previewCard;
            draggedCollectionCard = sourceType == DragSourceType.Collection
                ? new UserCollectionCardDto { CollectionCardId = collectionCardId, Card = previewCard }
                : null;
            draggedDeckCard = deckCard;
            draggedCardSource = sourceElement;
            dragStartPosition = evt.position;
            activeDragPointerId = evt.pointerId;
            isDraggingCard = false;

            draggedCardSource.CapturePointer(evt.pointerId);
        }

        private void OnCardDragMove(PointerMoveEvent evt)
        {
            if (draggedPreviewCard == null || draggedCardSource == null || evt.pointerId != activeDragPointerId)
                return;

            if (!isDraggingCard)
            {
                if (Vector2.Distance(evt.position, dragStartPosition) < 6f)
                    return;

                StartCardDragGhost();
                isDraggingCard = true;
            }

            UpdateCardDragGhost(evt.position);
            SetDeckDropZoneHighlight(IsPointerOverDeckDropZone(evt.position));
        }

        private void OnCardDragEnd(PointerUpEvent evt)
        {
            if (draggedPreviewCard == null || draggedCardSource == null || evt.pointerId != activeDragPointerId)
                return;

            if (isDraggingCard)
            {
                bool isOverDeckZone = IsPointerOverDeckDropZone(evt.position);

                if (isOverDeckZone && activeDragSource == DragSourceType.Collection)
                {
                    AddCardToSelectedDeck(draggedCollectionCard);
                }
                else if (!isOverDeckZone && activeDragSource == DragSourceType.Deck)
                {
                    RemoveCardFromSelectedDeck(draggedDeckCard);
                }
            }

            CancelCardDrag(true);
        }

        private void OnCardDragCancel(PointerCancelEvent evt)
        {
            if (evt == null || evt.pointerId != activeDragPointerId)
                return;

            CancelCardDrag(true);
        }

        private void StartCardDragGhost()
        {
            if (rootVisualElement == null || draggedCardSource == null || draggedPreviewCard == null)
                return;

            if (draggedCardGhost != null)
                draggedCardGhost.RemoveFromHierarchy();

            draggedCardGhost = cardTemplate.CloneTree();
            draggedCardGhost.pickingMode = PickingMode.Ignore;
            draggedCardGhost.style.position = Position.Absolute;
            draggedCardGhost.style.opacity = 0.85f;
            draggedCardGhost.style.width = draggedCardSource.resolvedStyle.width;
            draggedCardGhost.style.height = draggedCardSource.resolvedStyle.height;
            draggedCardGhost.style.left = dragStartPosition.x + 16f;
            draggedCardGhost.style.top = dragStartPosition.y + 16f;

            BindCardVisual(draggedCardGhost, draggedPreviewCard);
            rootVisualElement.Add(draggedCardGhost);
        }

        private void UpdateCardDragGhost(Vector2 panelPosition)
        {
            if (draggedCardGhost == null)
                return;

            draggedCardGhost.style.left = panelPosition.x + 16f;
            draggedCardGhost.style.top = panelPosition.y + 16f;
        }

        private void SetDeckDropZoneHighlight(bool highlighted)
        {
            if (deckDropZone == null || isDeckDropZoneHighlighted == highlighted)
                return;

            isDeckDropZoneHighlighted = highlighted;
            deckDropZone.style.backgroundColor = highlighted
                ? new Color(0.92f, 0.96f, 1f, 0.85f)
                : Color.white;
        }

        private bool IsPointerOverDeckDropZone(Vector2 panelPosition)
        {
            if (deckDropZone == null || rootVisualElement?.panel == null)
                return false;

            return deckDropZone.worldBound.Contains(panelPosition);
        }

        private void CancelCardDrag(bool resetHighlight)
        {
            if (draggedCardSource != null && activeDragPointerId >= 0)
                draggedCardSource.ReleasePointer(activeDragPointerId);

            activeDragSource = DragSourceType.None;
            draggedPreviewCard = null;
            draggedCollectionCard = null;
            draggedDeckCard = null;
            draggedCardSource = null;
            activeDragPointerId = -1;
            isDraggingCard = false;

            if (draggedCardGhost != null)
            {
                draggedCardGhost.RemoveFromHierarchy();
                draggedCardGhost = null;
            }

            if (resetHighlight)
                SetDeckDropZoneHighlight(false);
        }

        private void AddCardToSelectedDeck(UserCollectionCardDto cardData)
        {
            if (cardData?.Card == null || cardData.CollectionCardId == Guid.Empty || currentDeckId == Guid.Empty)
                return;

            DeckCardDto existing = currentDeckCards.Find(card => card.CollectionCardId == cardData.CollectionCardId);
            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                currentDeckCards.Add(new DeckCardDto
                {
                    CollectionCardId = cardData.CollectionCardId,
                    CardId = cardData.Card.Id,
                    Name = cardData.Card.Name,
                    Hp = cardData.Card.Hp,
                    Attack = cardData.Card.Attack,
                    Cost = cardData.Card.Cost,
                    Description = cardData.Card.Description,
                    Picture = cardData.Card.Picture,
                    Extension = cardData.Card.Extension,
                    CardType = cardData.Card.CardType,
                    Price = cardData.Card.Price,
                    Classes = cardData.Card.Class ?? new List<string>(),
                    Quantity = 1,
                    Rarity = cardData.OwnData != null && cardData.OwnData.Count > 0 ? cardData.OwnData[0].Rarity : ""
                });
            }

            ShowSelectedDeckCards(currentDeckCards);
            PersistDeckChanges();
        }

        private void RemoveCardFromSelectedDeck(DeckCardDto deckCard)
        {
            if (deckCard == null || deckCard.CollectionCardId == Guid.Empty || currentDeckId == Guid.Empty)
                return;

            DeckCardDto existing = currentDeckCards.Find(card => card.CollectionCardId == deckCard.CollectionCardId);
            if (existing == null)
                return;

            if (existing.Quantity > 1)
                existing.Quantity -= 1;
            else
                currentDeckCards.Remove(existing);

            ShowSelectedDeckCards(currentDeckCards);
            PersistDeckChanges();
        }

        private void PersistDeckChanges()
        {
            if (deckService == null || currentDeckId == Guid.Empty)
                return;

            UpdateDeckDto payload = new UpdateDeckDto
            {
                Name = currentDeckName,
                ChampionId = currentChampionId,
                FactionId = currentFactionId,
                Cards = currentDeckCards.Select(card => new UpdateDeckCardDto
                {
                    CollectionCardId = card.CollectionCardId,
                    Quantity = card.Quantity
                }).ToList()
            };

            StartCoroutine(deckService.UpdateDeckAsync(
                currentDeckId,
                payload,
                onSuccess: () => Debug.Log("[CollectionUI] Deck mis a jour"),
                onError: error => Debug.LogError(error)
            ));
        }

        private void InitializeDeckNameEditor()
        {
            if (deckNameContainer == null || deckNameLabel == null || editDeckNameButton == null)
                return;

            deckNameContainer.style.display = DisplayStyle.Flex;
            deckNameContainer.style.alignItems = Align.Center;
            deckNameContainer.style.justifyContent = Justify.FlexStart;

            // Keep the label compact so the edit button stays next to it
            deckNameLabel.style.flexGrow = 0f;
            deckNameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            editDeckNameButton.style.flexShrink = 0f;
            editDeckNameButton.text = string.Empty;
            editDeckNameButton.tooltip = "Edit deck name";
            editDeckNameButton.style.marginLeft = 6;

            deckNameTextField = rootVisualElement.Q<TextField>("DeckNameInput");
            if (deckNameTextField != null)
            {
                deckNameTextField.isDelayed = false;
                deckNameTextField.style.display = DisplayStyle.None;
                deckNameTextField.style.flexGrow = 1f;
                deckNameTextField.style.marginRight = 8;
                deckNameTextField.RegisterCallback<KeyDownEvent>(OnDeckNameInputKeyDown);
                deckNameTextField.RegisterCallback<BlurEvent>(_ => CommitDeckNameEdit());
            }

            editDeckNameButton.clicked += ToggleDeckNameEditMode;
            RefreshDeckNameDisplay();
        }

        private void ToggleDeckNameEditMode()
        {
            if (currentDeckId == Guid.Empty || deckNameLabel == null || deckNameTextField == null)
                return;

            bool isEditing = deckNameTextField.style.display == DisplayStyle.Flex;
            if (isEditing)
            {
                CommitDeckNameEdit();
                return;
            }

            deckNameTextField.value = currentDeckName;
            SetDeckNameEditMode(true);
            deckNameTextField.schedule.Execute(() =>
            {
                deckNameTextField.Focus();
                deckNameTextField.SelectAll();
            });
        }

        private void OnDeckNameInputKeyDown(KeyDownEvent evt)
        {
            if (evt == null)
                return;

            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                evt.StopPropagation();
                CommitDeckNameEdit();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                evt.StopPropagation();
                CancelDeckNameEdit();
            }
        }

        private void CommitDeckNameEdit()
        {
            if (deckNameTextField == null)
                return;

            string newName = deckNameTextField.value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(newName))
                newName = "Deck";

            SetCurrentDeckName(newName);
            SetDeckNameEditMode(false);

            if (currentDeckId != Guid.Empty)
                PersistDeckChanges();
        }

        private void CommitDeckNameEditIfNeeded()
        {
            if (deckNameTextField == null)
                return;

            if (deckNameTextField.style.display == DisplayStyle.Flex)
                CommitDeckNameEdit();
        }

        private void CancelDeckNameEdit()
        {
            if (deckNameTextField != null)
                deckNameTextField.value = currentDeckName;

            SetDeckNameEditMode(false);
        }

        private void SetCurrentDeckName(string deckName)
        {
            currentDeckName = string.IsNullOrWhiteSpace(deckName) ? "Deck" : deckName;
            RefreshDeckNameDisplay();

            if (deckNameContainer != null)
                deckNameContainer.style.display = DisplayStyle.Flex;

            if (selectedDeckButton != null)
                selectedDeckButton.text = currentDeckName;
        }

        private void RefreshDeckNameDisplay()
        {
            if (deckNameLabel != null)
                deckNameLabel.text = string.IsNullOrWhiteSpace(currentDeckName) ? "Deck" : currentDeckName;

            if (deckNameTextField != null)
                deckNameTextField.value = string.IsNullOrWhiteSpace(currentDeckName) ? "Deck" : currentDeckName;
        }

        private void SetDeckNameEditMode(bool isEditing)
        {
            if (deckNameLabel != null)
                deckNameLabel.style.display = isEditing ? DisplayStyle.None : DisplayStyle.Flex;

            if (deckNameTextField != null)
                deckNameTextField.style.display = isEditing ? DisplayStyle.Flex : DisplayStyle.None;

            if (editDeckNameButton != null)
                editDeckNameButton.tooltip = isEditing ? "Save deck name" : "Edit deck name";
        }

        private static CardDto CreateDeckPreviewCard(DeckCardDto card)
        {
            return new CardDto
            {
                Id = card.CardId,
                Name = card.Name,
                Price = card.Price,
                Hp = card.Hp ?? 0,
                Attack = card.Attack ?? 0,
                Cost = card.Cost,
                Description = card.Description,
                Picture = card.Picture,
                Extension = card.Extension,
                CardType = card.CardType,
                Class = card.Classes ?? new List<string>()
            };
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
