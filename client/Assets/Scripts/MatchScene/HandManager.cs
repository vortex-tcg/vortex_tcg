using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HandManager : MonoBehaviour
{
    [Header("UI Toolkit References")]
    [SerializeField] private VisualTreeAsset SmallCard;     // pour la main
    [SerializeField] private VisualTreeAsset CardPreview;   // pour la preview
    private VisualElement handZone;
    private VisualElement previewZone;

    private List<CardDTO> playerHand = new();

    private void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;

        var root = uiDoc.rootVisualElement;
        if (root == null) return;

        handZone = root.Q<VisualElement>("P1CardsFrame");
        previewZone = root.Q<VisualElement>("CardPreview");

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

            SetLabel(cardElement, "Name", card.Name);
            SetLabel(cardElement, "Cost", card.Cost.ToString());

            cardElement.RegisterCallback<MouseEnterEvent>(_ =>
            {
                ShowPreview(card);
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
        previewZone.style.display = DisplayStyle.None;
    }

    private void SetLabel(VisualElement parent, string name, string value)
    {
        var label = parent.Q<Label>(name);
        if (label != null) label.text = value;
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
            Description = "Inflige 1 dégât à tous les ennemis.",
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
            Description = "Provocation. Réduit les dégâts subis de 1.",
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
            Description = "Inflige 4 dégâts à une cible.",
            CardType = CardType.Spell,
            Class = new List<string> { "Mage" },
            Effects = new List<Effect>()
        },
        new CardDTO
        {
            Id = Guid.NewGuid(),
            GameId = 1,
            Name = "Archère elfe",
            Hp = 2,
            Attack = 3,
            Cost = 2,
            Description = "Inflige 1 dégât supplémentaire si votre héros a moins de 10 PV.",
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
            Description = "Provocation. Inflige 1 dégât à tous les ennemis adjacents.",
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
            Description = "Inflige 6 dégâts répartis aléatoirement entre tous les ennemis.",
            CardType = CardType.Spell,
            Class = new List<string> { "Mage" },
            Effects = new List<Effect>()
        }
    };
    }
}

    // -----------------------------
    // STRUCTURES DE DONNÉES
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
