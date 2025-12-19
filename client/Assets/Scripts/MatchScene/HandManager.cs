using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using DrawDTOs;

namespace VortexTCG.Script.MatchScene {
    public class HandManager : MonoBehaviour
    {
    private VisualElement _root;

    private VisualElement handZone;
    private VisualElement previewZone;
    private VisualElement boardZone;
    private VisualElement enemyBoardZone;

    private List<VisualElement> boardSlots = new List<VisualElement>();
    private List<VisualElement> enemySlots = new List<VisualElement>();
    private List<DrawnCardDto> playerHand = new List<DrawnCardDto>();

    private VisualElement draggedElement;
    private DrawnCardDto draggedCard;
    private bool isDragging;
    private Vector2 dragOffset;

        private VisualElement draggedElement;
        private CardDTO draggedCard;
        private bool isDragging;
        private Vector2 dragOffset;

        private VisualElement originalParent;
        private int originalIndex;

        private DefenseManager defenseManager;

    private void OnEnable()
    {
        Debug.Log("[HandManager] onenable");
        UIDocument uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;

        _root = uiDoc.rootVisualElement;
        if (_root == null) return;

        handZone = _root.Q<VisualElement>("P1CardsFrame");
        previewZone = _root.Q<VisualElement>("CardPreview");
        boardZone = _root.Q<VisualElement>("P1BoardCards");
        enemyBoardZone = _root.Q<VisualElement>("P2BoardCards");

            handZone = root.Q<VisualElement>("P1CardsFrame");
            previewZone = root.Q<VisualElement>("CardPreview");
            VisualElement boardZone = root.Q<VisualElement>("P1BoardCards");
            VisualElement enemyBoardZone = root.Q<VisualElement>("P2BoardCards");

            if (boardZone != null)
                boardSlots = boardZone.Query<VisualElement>(className: "P1Slot").ToList();

        _root.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        _root.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        _root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        _root.RegisterCallback<PointerUpEvent>(OnPointerUp);

            root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            root.RegisterCallback<PointerUpEvent>(OnPointerUp);

        if (handZone == null || previewZone == null || SmallCard == null || CardPreview == null)
            return;
    }

    public void SetHand(List<DrawnCardDto> cards)
    {
        Debug.Log($"[HandManager] SetHand called. cards={(cards == null ? "NULL" : cards.Count.ToString())}");

        playerHand = cards ?? new List<DrawnCardDto>();
        GenerateHand(playerHand);
        ClearPreview();

        Debug.Log($"[HandManager] Hand UI generated. children={handZone?.childCount}");
    }

    public void AddCards(List<DrawnCardDto> newCards)
    {
        Debug.Log($"[HandManager] AddCards called. cards={(newCards == null ? "NULL" : newCards.Count.ToString())}");

        if (newCards == null || newCards.Count == 0) return;

        if (playerHand == null) playerHand = new List<DrawnCardDto>();
        playerHand.AddRange(newCards);

        GenerateHand(playerHand);
        ClearPreview();
    }

    private void GenerateHand(List<DrawnCardDto> cards)
    {
        if (handZone == null)
        {
            Debug.LogError("[HandManager] handZone NULL -> UIDocument/root.Q failed ?");
            return;
        }

        Debug.Log($"[HandManager] GenerateHand. count={cards?.Count ?? -1}");

        handZone.Clear();

        foreach (DrawnCardDto card in cards)
        {
            handZone.Clear();

            foreach (CardDTO card in cards)
            {
                VisualElement cardElement = SmallCard.Instantiate();
                if (cardElement == null) continue;

                cardElement.userData = false;

                SetLabel(cardElement, "Name", card.Name);
                SetLabel(cardElement, "Cost", card.Cost.ToString());

                cardElement.RegisterCallback<MouseEnterEvent>(_ => ShowPreview(card));

                cardElement.RegisterCallback<PointerDownEvent>(e =>
                {
                    if (CanDrag)
                        StartDrag(cardElement, card, e);
                    e.StopPropagation();
                });

                cardElement.RegisterCallback<MouseLeaveEvent>(_ => ClearPreview());

                handZone.Add(cardElement);
            }
        }

    private void ShowPreview(DrawnCardDto card)
    {
        previewZone.Clear();
        VisualElement previewCard = CardPreview.Instantiate();

            SetLabel(previewCard, "Name", card.Name);
            SetLabel(previewCard, "Cost", card.Cost.ToString());
            SetLabel(previewCard, "ATKPoints", card.Attack > 0 ? card.Attack.ToString() : "-");
            SetLabel(previewCard, "DEFPoints", card.Hp > 0 ? card.Hp.ToString() : "-");
            SetLabel(previewCard, "LoreDesc", card.Description);

            previewZone.Add(previewCard);
            previewZone.style.display = DisplayStyle.Flex;
        }

        private void ClearPreview()
        {
            previewZone.Clear();
            if (EmptyCardPreview != null)
            {
                VisualElement emptyPreview = EmptyCardPreview.Instantiate();
                previewZone.Add(emptyPreview);
            }
            previewZone.style.display = DisplayStyle.Flex;
        }

        private void SetLabel(VisualElement parent, string name, string value)
        {
            Label label = parent.Q<Label>(name);
            if (label != null)
                label.text = value;
        }

    private void RemoveFromHand(DrawnCardDto card)
    {
        if (card == null || playerHand == null) return;
        playerHand.RemoveAll(c => c.GameCardId == card.GameCardId);
    }

    private void StartDrag(VisualElement cardElement, DrawnCardDto card, PointerDownEvent e)
    {
        draggedElement = cardElement;
        draggedCard = card;
        isDragging = true;

        originalParent = cardElement.parent;
        originalIndex = originalParent != null ? originalParent.IndexOf(cardElement) : 0;

            Vector2 mousePos = e.position;
            Vector2 elementWorldPos = cardElement.worldBound.position;
            dragOffset = mousePos - elementWorldPos;

            draggedElement.style.position = Position.Absolute;
            draggedElement.BringToFront();

            Vector2 parentWorldPos = draggedElement.parent.worldBound.position;
            Vector2 local = mousePos - parentWorldPos - dragOffset;

            draggedElement.style.left = local.x;
            draggedElement.style.top = local.y;
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            if (!isDragging || draggedElement == null) return;

            Vector2 mousePos = e.position;
            Vector2 parentWorldPos = draggedElement.parent.worldBound.position;
            Vector2 local = mousePos - parentWorldPos - dragOffset;

            draggedElement.style.left = local.x;
            draggedElement.style.top = local.y;
        }

        private void OnPointerUp(PointerUpEvent e)
        {
            if (!isDragging || draggedElement == null) return;

            isDragging = false;
            Vector2 mousePos = e.position;

        if (_root == null) return;

        _root.schedule.Execute(() =>
        {
            bool dropped = false;

            if (PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase == GamePhase.StandBy)
            {
                dropped = TryDropOnBoard(mousePos, boardSlots);
                if (dropped) RemoveFromHand(draggedCard);
            }
            else if (PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase == GamePhase.Defense)
            {
                VisualElement enemySlot = enemySlots.FirstOrDefault(slot =>
                    slot.childCount > 0 && slot.worldBound.Contains(mousePos));

                if (enemySlot != null)
                {
                    VisualElement enemyCard = enemySlot.Q<VisualElement>(className: "small-card");
                    if (enemyCard != null && defenseManager != null)
                        defenseManager.TryAssignDefense(draggedElement, enemyCard);

                    ResetCardPosition(draggedElement);
                    ClearDrag();
                    return;
                }
            }

            if (!dropped)
                ResetCardPosition(draggedElement);

            ClearDrag();
        });
    }

        private void ResetCardPosition(VisualElement cardElement)
        {
            if (originalParent != null)
            {
                cardElement.RemoveFromHierarchy();
                originalParent.Insert(originalIndex, cardElement);
            }

            cardElement.style.position = Position.Relative;
            cardElement.style.left = StyleKeyword.Null;
            cardElement.style.top = StyleKeyword.Null;
        }

        private bool TryDropOnBoard(Vector2 mousePos, List<VisualElement> slots, bool requireCardInSlot = false)
        {
            foreach (VisualElement slot in slots)
            {
                if (slot == null) continue;
                if (!slot.worldBound.Contains(mousePos)) continue;

                if (!requireCardInSlot && slot.childCount > 0) return false;
                if (requireCardInSlot && slot.childCount == 0) continue;

                draggedElement.RemoveFromHierarchy();
                slot.Add(draggedElement);

                draggedElement.style.position = Position.Relative;
                draggedElement.style.left = StyleKeyword.Null;
                draggedElement.style.top = StyleKeyword.Null;

                draggedElement.userData = true;
                draggedElement.AddToClassList("on-board");

                AttackManager attackManager = FindObjectOfType<AttackManager>();
                if (attackManager != null)
                    attackManager.RegisterCard(draggedElement);

                return true;
            }

            return false;
        }


    private void ClearDrag()
    {
        public Guid Id;
        public string Name;
        public int Hp;
        public int Attack;
        public int Cost;
        public string Description;
        public CardType CardType;
    }
}
