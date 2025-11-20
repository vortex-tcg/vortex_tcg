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
        if (boardZone !=null)
        {
            boardSlots = boardZone.Query<VisualElement>("Card").ToList();
        }

        if (handZone == null || previewZone == null || SmallCard == null || CardPreview == null) return;

        playerHand = MockFetchPlayerHand();
        GenerateHand(playerHand);
    }

    private void GenerateHand(List<CardDTO> cards)
    {
        if (handZone == null || SmallCard == null || cards == null) return;

        handZone.Clear();

        foreach (var card in cards)
        {
            var cardElement = SmallCard.Instantiate();
            if (cardElement == null) continue;

            cardElement.userData = false;
            SetLabel(cardElement, "Name", card.Name);
            SetLabel(cardElement, "Cost", card.Cost.ToString());

            cardElement.RegisterCallback<MouseEnterEvent>(_ =>
            {
                ShowPreview(card);
            });
            cardElement.RegisterCallback<PointerDownEvent>(e =>
            {
                StartDrag(cardElement, card, e);
                e.StopPropagation();
            });


            cardElement.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                ClearPreview();
            });

            handZone.Add(cardElement);
        }
    }

    private void ShowPreview(CardDTO card)
    {
        if (previewZone == null || CardPreview == null) return;

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
        if (previewZone == null) return;
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
        if (cardElement.userData is bool isLocked && isLocked)
        {
            return;
        }
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
        draggedElement.style.top  = local.y;
    }


    private void OnPointerMove(PointerMoveEvent e)
    {
        if (!isDragging || draggedElement == null) return;
        Vector2 mousePos = e.position;                            
        Vector2 parentWorldPos = draggedElement.parent.worldBound.position; 
        Vector2 local = mousePos - parentWorldPos - dragOffset;
        draggedElement.style.left = local.x;
        draggedElement.style.top  = local.y;
    }


    private bool TryDropOnBoard(Vector2 mousePos)
    {
        if (boardSlots == null || boardSlots.Count == 0)
            return false;
        foreach (var slot in boardSlots)
        {
            if (slot == null) continue;

            if (slot.worldBound.Contains(mousePos))
            {
                if (slot.childCount > 0)
                {
                    return false;
                }

                if (draggedElement.parent != slot)
                    draggedElement.RemoveFromHierarchy();

                slot.Add(draggedElement);
                draggedElement.style.position = Position.Relative;
                draggedElement.style.left = StyleKeyword.Null;
                draggedElement.style.top = StyleKeyword.Null;
                draggedElement.userData = true;
                return true;
            }
        }

        return false;
    }

    private void OnPointerUp(PointerUpEvent e)
    {
        if (!isDragging || draggedElement == null)
            return;

        isDragging = false;

        Vector2 mousePos = e.position;
        if (TryDropOnBoard(mousePos))
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
            if (cardElement.parent != originalParent)
                cardElement.RemoveFromHierarchy();

            originalParent.Insert(originalIndex, cardElement);
        }

        cardElement.style.position = Position.Relative;
        cardElement.style.left = StyleKeyword.Null;
        cardElement.style.top = StyleKeyword.Null;
    }

    // -----------------------------
    // MOCK TEMPORAIRE
    // -----------------------------
    private List<CardDTO> MockFetchPlayerHand()
    {
        return new List<CardDTO>
    {
        new CardDTO
        {
            Id = Guid.NewGuid(),
            GameId = 1,
            Name = "Pyromancien novice",
            Hp = 3,
            Attack = 2,
            Cost = 1,
            Description = "Inflige 1 d�g�t � tous les ennemis.",
            CardType = CardType.Creature,
            Class = new List<string> { "Mage" },
            Effects = new List<Effect>()
        },
        new CardDTO
        {
            Id = Guid.NewGuid(),
            GameId = 1,
            Name = "Garde de pierre",
            Hp = 6,
            Attack = 1,
            Cost = 2,
            Description = "Provocation. R�duit les d�g�ts subis de 1.",
            CardType = CardType.Creature,
            Class = new List<string> { "Guerrier" },
            Effects = new List<Effect>()
        },
        new CardDTO
        {
            Id = Guid.NewGuid(),
            GameId = 1,
            Name = "Boule de feu",
            Hp = 0,
            Attack = 0,
            Cost = 3,
            Description = "Inflige 4 d�g�ts � une cible.",
            CardType = CardType.Spell,
            Class = new List<string> { "Mage" },
            Effects = new List<Effect>()
        },
        new CardDTO
        {
            Id = Guid.NewGuid(),
            GameId = 1,
            Name = "Arch�re elfe",
            Hp = 2,
            Attack = 3,
            Cost = 2,
            Description = "Inflige 1 d�g�t suppl�mentaire si votre h�ros a moins de 10 PV.",
            CardType = CardType.Creature,
            Class = new List<string> { "Archer" },
            Effects = new List<Effect>()
        },
        new CardDTO
        {
            Id = Guid.NewGuid(),
            GameId = 1,
            Name = "Soin mineur",
            Hp = 0,
            Attack = 0,
            Cost = 1,
            Description = "Restaure 3 points de vie à une cible.",
            CardType = CardType.Spell,
            Class = new List<string> { "Clerc" },
            Effects = new List<Effect>()
        },
        new CardDTO
        {
            Id = Guid.NewGuid(),
            GameId = 1,
            Name = "Chevalier de lumière",
            Hp = 5,
            Attack = 4,
            Cost = 4,
            Description = "Provocation. Inflige 1 dégàt à tous les ennemis adjacents.",
            CardType = CardType.Creature,
            Class = new List<string> { "Paladin" },
            Effects = new List<Effect>()
        },
        new CardDTO
        {
            Id = Guid.NewGuid(),
            GameId = 1,
            Name = "Explosion arcanique",
            Hp = 0,
            Attack = 0,
            Cost = 5,
            Description = "Inflige 6 d�g�ts r�partis al�atoirement entre tous les ennemis.",
            CardType = CardType.Spell,
            Class = new List<string> { "Mage" },
            Effects = new List<Effect>()
        }
    };
    }
}

    // -----------------------------
    // STRUCTURES DE DONNéES
    // -----------------------------
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