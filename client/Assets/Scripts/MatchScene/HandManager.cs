using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class HandManager : MonoBehaviour
{
    [Header("UI Toolkit References")]
    [SerializeField] private VisualTreeAsset SmallCard;
    [SerializeField] private VisualTreeAsset CardPreview;
    [SerializeField] private VisualTreeAsset EmptyCardPreview;

    private VisualElement handZone;
    private VisualElement previewZone;
    private VisualElement boardZone;
    private VisualElement enemyBoardZone;

    private List<VisualElement> boardSlots = new();
    private List<VisualElement> enemySlots = new();
    private List<CardDTO> playerHand = new();

    private VisualElement draggedElement;
    private CardDTO draggedCard;
    private bool isDragging;
    private Vector2 dragOffset;

    private VisualElement originalParent;
    private int originalIndex;

    private bool CanDrag => PhaseManager.Instance != null &&
                            (PhaseManager.Instance.CurrentPhase == GamePhase.StandBy ||
                             PhaseManager.Instance.CurrentPhase == GamePhase.Defense);

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;

        var root = uiDoc.rootVisualElement;
        if (root == null) return;

        handZone = root.Q<VisualElement>("P1CardsFrame");
        previewZone = root.Q<VisualElement>("CardPreview");
        boardZone = root.Q<VisualElement>("P1BoardCards");
        enemyBoardZone = root.Q<VisualElement>("P2BoardCards");

        if (boardZone != null)
            boardSlots = boardZone.Query<VisualElement>("Card").ToList();
        if (enemyBoardZone != null)
            enemySlots = enemyBoardZone.Query<VisualElement>("Card").ToList();

        root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        root.RegisterCallback<PointerUpEvent>(OnPointerUp);

        if (handZone == null || previewZone == null || SmallCard == null || CardPreview == null) return;

        playerHand = MockFetchPlayerHand();
        GenerateHand(playerHand);
    }

    private void GenerateHand(List<CardDTO> cards)
    {
        handZone.Clear();

        foreach (var card in cards)
        {
            var cardElement = SmallCard.Instantiate();
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

    private void ShowPreview(CardDTO card)
    {
        previewZone.Clear();
        var previewCard = CardPreview.Instantiate();

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
            var emptyPreview = EmptyCardPreview.Instantiate();
            previewZone.Add(emptyPreview);
        }
        previewZone.style.display = DisplayStyle.Flex;
    }

    private void SetLabel(VisualElement parent, string name, string value)
    {
        var label = parent.Q<Label>(name);
        if (label != null) label.text = value;
    }

    private void StartDrag(VisualElement cardElement, CardDTO card, PointerDownEvent e)
    {
        draggedElement = cardElement;
        draggedCard = card;
        isDragging = true;

        originalParent = cardElement.parent;
        originalIndex = originalParent.IndexOf(cardElement);

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

        if (PhaseManager.Instance.CurrentPhase == GamePhase.StandBy)
        {
            if (TryDropOnBoard(mousePos, boardSlots))
            {
                ClearDrag();
                return;
            }
        }
        else if (PhaseManager.Instance.CurrentPhase == GamePhase.Defense)
        {
            var enemyCard = enemySlots.FirstOrDefault(slot =>
                slot.childCount > 0 && slot.worldBound.Contains(mousePos));
            if (enemyCard != null)
            {

                TakeAttackAction(draggedCard, enemyCard);
                ResetCardPosition(draggedElement);
                ClearDrag();
                return;
            }
        }

        ResetCardPosition(draggedElement);
        ClearDrag();
    }

    private void TakeAttackAction(CardDTO myCard, VisualElement enemyCard)
    {
        Debug.Log($"Carte '{myCard.Name}' bloque l'attaque de '{enemyCard.Q<Label>("Name")?.text}'");
    }

    private void ClearDrag()
    {
        draggedElement = null;
        draggedCard = null;
    }


    private bool TryDropOnBoard(Vector2 mousePos, List<VisualElement> slots, bool requireCardInSlot = false)
    {
        foreach (var slot in slots)
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
            return true;
        }

        return false;
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


    private List<CardDTO> MockFetchPlayerHand()
    {
        return new List<CardDTO>
        {
            new CardDTO{ Id=Guid.NewGuid(), Name="Pyromancien", Hp=3, Attack=2, Cost=1, Description="Inflige 1 dmg.", CardType=CardType.Creature },
            new CardDTO{ Id=Guid.NewGuid(), Name="Garde", Hp=6, Attack=1, Cost=2, Description="Provocation.", CardType=CardType.Creature },
            new CardDTO{ Id=Guid.NewGuid(), Name="Boule de feu", Hp=0, Attack=0, Cost=3, Description="Inflige 4 dmg.", CardType=CardType.Spell },
            new CardDTO{ Id=Guid.NewGuid(), Name="Archère", Hp=2, Attack=3, Cost=2, Description="Bonus si PV < 10.", CardType=CardType.Creature },
            new CardDTO{ Id=Guid.NewGuid(), Name="Soin mineur", Hp=0, Attack=0, Cost=1, Description="Restaure 3 PV.", CardType=CardType.Spell }
        };
    }
}

[Serializable]
public class CardDTO
{
    public Guid Id;
    public string Name;
    public int Hp;
    public int Attack;
    public int Cost;
    public string Description;
    public CardType CardType;
}

public enum CardType
{
    Creature,
    Spell,
    Artifact
}
