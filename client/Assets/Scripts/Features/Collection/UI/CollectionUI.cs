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
    public partial class CollectionUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private UIDocument deleteModalDocument;
        [SerializeField] private UIDocument createModalDocument;
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
        private Button addDeckButton;
        private VisualElement deckNameContainer;
        private Label deckNameLabel;
        private Button editDeckNameButton;
        private Button deleteDeckButton;
        private Label deckLengthLabel;
        private Button backButton;
        private TextField deckNameTextField;
        private Button selectedDeckButton;
        private VisualElement selectedDeckCardsContainer;
        private VisualElement deleteModalHUD;
        private Label deleteModalDeckNameLabel;
        private Button deleteModalConfirmButton;
        private Button deleteModalCancelButton;
        private VisualElement createModalHUD;
        private TextField createDeckInput;
        private Button createModalConfirmButton;
        private Button createModalCancelButton;
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

            if (deleteModalDocument == null)
            {
                GameObject deleteModalGameObject = GameObject.Find("DeleteModalHUD");
                if (deleteModalGameObject != null)
                    deleteModalDocument = deleteModalGameObject.GetComponent<UIDocument>();
            }

            if (createModalDocument == null)
            {
                GameObject createModalGameObject = GameObject.Find("CreateModalHUD");
                if (createModalGameObject != null)
                    createModalDocument = createModalGameObject.GetComponent<UIDocument>();
            }

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
            addDeckButton = rootVisualElement.Q<Button>("Add");
            deckNameContainer = rootVisualElement.Q<VisualElement>("DeckNameContainer");
            deckNameLabel = rootVisualElement.Q<Label>("DeckName");
            editDeckNameButton = rootVisualElement.Q<Button>("EditDeckName");
            deleteDeckButton = rootVisualElement.Q<Button>("DeleteDeck");
            deckLengthLabel = rootVisualElement.Q<Label>("DeckLength");
            backButton = rootVisualElement.Q<Button>("BackButton");
            selectedDeckCardsContainer = rootVisualElement.Q<VisualElement>("SelectedCardsContainer");
            deckDropZone = rootVisualElement.Q<VisualElement>("DragAndDropZone");
            deleteModalHUD = deleteModalDocument != null ? deleteModalDocument.rootVisualElement : null;
            deleteModalDeckNameLabel = deleteModalHUD?.Q<Label>("DeckName");
            deleteModalConfirmButton = deleteModalHUD?.Q<Button>("ConfirmButton");
            deleteModalCancelButton = deleteModalHUD?.Q<Button>("CancelButton");
            createModalHUD = createModalDocument != null ? createModalDocument.rootVisualElement : null;
            createDeckInput = createModalHUD?.Q<TextField>("CreateDeckInput");
            createModalConfirmButton = createModalHUD?.Q<Button>("ConfirmButton");
            createModalCancelButton = createModalHUD?.Q<Button>("CancelButton");

            InitializeDeckNameEditor();
            InitializeDeleteDeckModal();
            InitializeCreateDeckModal();
            InitializeBackButton();

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

        private void InitializeBackButton()
        {
            if (backButton != null)
            {
                backButton.clicked += OnBackClicked;
            }
        }

        private void OnBackClicked()
        {
            LoadingScreen.Load("HomeScene", loadMenu: true, unloadMenu: false);
        }
    }
}
