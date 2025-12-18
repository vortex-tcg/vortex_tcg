using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace VortexTCG.Script.Card {

    public class P2BoardManager : MonoBehaviour
    {
        [Header("Card UI")]
        [SerializeField] private VisualTreeAsset SmallCard;
    
        private List<VisualElement> slotsP2 = new();
    
        private void OnEnable()
        {
            UIDocument uiDoc = GetComponent<UIDocument>();
            if (uiDoc == null) return;
    
            VisualElement root = uiDoc.rootVisualElement;
            if (root == null) return;
    
            VisualElement boardZoneP2 = root.Q<VisualElement>("P2BoardCards");
            slotsP2 = boardZoneP2.Query<VisualElement>("P2Slot").ToList();
    
            List<CardDto> p2Cards = MockFetchP2BoardCards();
            GenerateP2Board(p2Cards);
        }
    
        private void ApplyRandomEngaged()
        {
            List<VisualElement> allCards = slotsP2
                .Select(s => s.Q<VisualElement>(className: "small-card"))
                .Where(c => c != null)
                .ToList();
    
            if (allCards.Count == 0)
                return;
    
            int randomIndex = UnityEngine.Random.Range(0, allCards.Count);
            VisualElement randomCard = allCards[randomIndex];
    
            randomCard.AddToClassList("engaged");
        }
    
    
        private void GenerateP2Board(List<CardDto> cards)
        {
            if (SmallCard == null)
            {
                return;
            }
    
            foreach (VisualElement slot in slotsP2)
                slot.Clear();
    
            int cardsToPlace = Mathf.Min(cards.Count, slotsP2.Count);
    
            for (int i = 0; i < cardsToPlace; i++)
            {
                CardDto card = cards[i];
                VisualElement slot = slotsP2[i];
    
                TemplateContainer cardElement = SmallCard.Instantiate();
                if (cardElement == null)
                    continue;
    
                SetLabel(cardElement, "Name", card.Name);
                SetLabel(cardElement, "Cost", card.Cost.ToString());
                SetLabel(cardElement, "ATKPoints", card.Attack.ToString());
                SetLabel(cardElement, "DEFPoints", card.Hp.ToString());
    
                slot.Add(cardElement);
                ApplyRandomEngaged();
            }
        }
    
        private static void SetLabel(VisualElement parent, string name, string value)
        {
            Label label = parent.Q<Label>(name);
            if (label != null)
                label.text = value;
        }
    
        private List<CardDto> MockFetchP2BoardCards()
        {
            return new List<CardDto>
            {
                new CardDto { Name = "Soldat squelette", Hp = 2, Attack = 2, Cost = 1 },
                new CardDto { Name = "Chaman sombre", Hp = 4, Attack = 3, Cost = 3 },
                new CardDto { Name = "Gargouille", Hp = 5, Attack = 1, Cost = 2 },
                new CardDto { Name = "Ombre furtive", Hp = 1, Attack = 4, Cost = 2 }
            };
        }
    
        [Serializable]
        private sealed class CardDto
        {
            public string Name;
            public int Hp;
            public int Attack;
            public int Cost;
        }
    }
}