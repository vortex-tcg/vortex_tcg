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

    private List<VisualElement> boardSlots = new();
    private List<CardDTO> playerHand = new();

    private VisualElement draggedElement;
    private CardDTO draggedCard;
    private bool isDragging;
    private Vector2 dragOffset;

    private VisualElement originalParent;
    private int originalIndex;

    private bool CanDrag =>
        PhaseManager.Instance != null &&
        PhaseManager.Instance.CurrentPhase == GamePhase.StandBy;

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;

        var root = uiDoc.rootVisualElement;
        if (root == null) return;

        handZone = root.Q<VisualElement>("P1CardsFrame");
        previewZone = root.Q<VisualElement>("CardPreview");

        root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        root.RegisterCallback<PointerUpEvent>(OnPointerUp);

        boardZone = root.Q<VisualElement>("P1BoardCards");
        if (boardZone != null)
            boardSlots = boardZone.Query<VisualElement>("Card").ToList();

        if (handZone == null || previewZone == null || SmallCard == null || CardPreview == null) return;

        playerHand = MockFetchPlayerHand();
        GenerateHand(playerHand);

        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnEnterStandBy += EnableHandDrag;
            PhaseManager.Instance.OnEnterAttack += DisableHandDrag;
            PhaseManager.Instance.OnEnterDefense += DisableHandDrag;
        }
    }

    private void EnableHandDrag()
    {
        // Rien à faire, CanDrag gère tout.
    }

    private void DisableHandDrag()
    {
        if (isDragging && draggedElement != null)
        {
            ResetCardPosition(draggedElement);
            draggedElement = null;
            draggedCard = null;
            isDragging = false;
        }
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
            previewZone.style.display = DisplayStyle.Flex;
        }
        else
        {
            previewZone.style.display = DisplayStyle.None;
        }
    }

    private void SetLabel(VisualElement parent, string name, string value)
    {
        var label = parent.Q<Label>(name);
        if (label != null) label.text = value;
    }


    private void StartDrag(VisualElement cardElement, CardDTO card, PointerDownEvent e)
    {
        if (!CanDrag) return;

        if (cardElement.userData is bool isLocked && isLocked)
            return;

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

    private bool TryDropOnBoard(Vector2 mousePos)
    {
        foreach (var slot in boardSlots)
        {
            if (slot == null) continue;
            if (!slot.worldBound.Contains(mousePos)) continue;
            if (slot.childCount > 0) return false;

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

    private void OnPointerUp(PointerUpEvent e)
    {
        if (!isDragging || draggedElement == null)
            return;

        isDragging = false;

        Vector2 mousePos = e.position;

        if (CanDrag && TryDropOnBoard(mousePos))
        {
            draggedElement = null;
            draggedCard = null;
            return;
        }

        ResetCardPosition(draggedElement);

        draggedElement = null;
        draggedCard = null;
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

    // -------- MOCK ----------
    private List<CardDTO> MockFetchPlayerHand()
    {
        return new List<CardDTO>
        {
            new CardDTO { Id = Guid.NewGuid(), Name = "Pyromancien novice", Hp = 3, Attack = 2, Cost = 1, Description = "Inflige 1 dégât à tous les ennemis.", CardType = CardType.Creature, Class = new List<string>{ "Mage" }, Effects = new List<Effect>() },
            new CardDTO { Id = Guid.NewGuid(), Name = "Garde de pierre", Hp = 6, Attack = 1, Cost = 2, Description = "Provocation.", CardType = CardType.Creature, Class = new List<string>{ "Guerrier" }, Effects = new List<Effect>() },
            new CardDTO { Id = Guid.NewGuid(), Name = "Boule de feu", Hp = 0, Attack = 0, Cost = 3, Description = "Inflige 4 dégâts.", CardType = CardType.Spell, Class = new List<string>{ "Mage" }, Effects = new List<Effect>() },
            new CardDTO { Id = Guid.NewGuid(), Name = "Archère elfe", Hp = 2, Attack = 3, Cost = 2, Description = "Bonus si PV < 10.", CardType = CardType.Creature, Class = new List<string>{ "Archer" }, Effects = new List<Effect>() },
            new CardDTO { Id = Guid.NewGuid(), Name = "Soin mineur", Hp = 0, Attack = 0, Cost = 1, Description = "Restaure 3 PV.", CardType = CardType.Spell, Class = new List<string>{ "Clerc" }, Effects = new List<Effect>() },
            new CardDTO { Id = Guid.NewGuid(), Name = "Chevalier de lumière", Hp = 5, Attack = 4, Cost = 4, Description = "Provocation.", CardType = CardType.Creature, Class = new List<string>{ "Paladin" }, Effects = new List<Effect>() },
            new CardDTO { Id = Guid.NewGuid(), Name = "Explosion arcanique", Hp = 0, Attack = 0, Cost = 5, Description = "6 dégâts aléatoires.", CardType = CardType.Spell, Class = new List<string>{ "Mage" }, Effects = new List<Effect>() }
        };
    }
}

[Serializable]
public class CardDTO
{
    public Guid Id;
    public int GameId;
    public string Name;
    public int Hp;
    public int Attack;
    public int Cost;
    public string Description;
    public CardType CardType;
    public ICollection<string> Class;
    public ICollection<Effect> Effects;
}

public enum CardType
{
    Creature,
    Spell,
    Artifact
}

[Serializable]
public class Effect
{
    public string EffectName;
    public string EffectDescription;
}
