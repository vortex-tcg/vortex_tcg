using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class DefenseManager : MonoBehaviour
{
    [SerializeField] private UIDocument boardDocument;
    private VisualElement p1BoardRoot;
    private readonly List<VisualElement> selectedCards = new List<VisualElement>();
    private readonly Dictionary<VisualElement, VisualElement> defenseAssignments = new Dictionary<VisualElement, VisualElement>();

    private void Start()
    {
        if (boardDocument == null)
            boardDocument = GetComponent<UIDocument>();

        VisualElement root = boardDocument.rootVisualElement;
        p1BoardRoot = root.Q<VisualElement>("P1BoardCards");

        RegisterAllExistingCards();

        PhaseManager.Instance.OnEnterDefense += OnEnterDefensePhase;
        PhaseManager.Instance.OnEnterStandBy += OnEndDefensePhase;
    }

    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnEnterDefense -= OnEnterDefensePhase;
            PhaseManager.Instance.OnEnterStandBy -= OnEndDefensePhase;
        }
    }

    private void OnEnterDefensePhase()
    {
        ClearSelections();
    }

    private void OnEndDefensePhase()
    {
        ClearSelections();
    }

    private void RegisterAllExistingCards()
    {
        List<VisualElement> allSlots = p1BoardRoot.Query<VisualElement>(className: "P1Slot").ToList();
        foreach (VisualElement slot in allSlots)
        {
            slot.RegisterCallback<ClickEvent>(_ =>
            {
                if (PhaseManager.Instance.CurrentPhase != GamePhase.Defense)
                    return;

                VisualElement card = slot.Q<VisualElement>(className: "small-card");
                if (card != null)
                    ToggleCard(card);
            });
        }
    }

    public void TryAssignDefense(VisualElement defenderCard, VisualElement targetCard)
    {
        if (PhaseManager.Instance.CurrentPhase != GamePhase.Defense)
            return;

        if (defenderCard == null || targetCard == null) return;
        if (defenderCard.parent == null || !defenderCard.parent.ClassListContains("P1Slot"))
            return;

        if (!targetCard.ClassListContains("engaged"))
            return;

        VisualElement existingDefender = defenseAssignments.FirstOrDefault(kvp => kvp.Value == targetCard).Key;
        if (existingDefender != null)
            return;

        ClearDefense(defenderCard);

        defenderCard.AddToClassList("defense");
        defenseAssignments[defenderCard] = targetCard;

        if (!selectedCards.Contains(defenderCard))
            selectedCards.Add(defenderCard);
    }

    private void ToggleCard(VisualElement card)
    {
        if (card == null) return;

        if (selectedCards.Contains(card))
            DeselectCard(card);
        else
            SelectCard(card);
    }

    private void SelectCard(VisualElement card)
    {
        if (card == null) return;

        selectedCards.Add(card);
        if (!card.ClassListContains("defense"))
            card.AddToClassList("defense");
    }

    private void DeselectCard(VisualElement card)
    {
        if (card == null) return;

        selectedCards.Remove(card);
        if (card.ClassListContains("defense"))
            card.RemoveFromClassList("defense");
    }

    private void ClearDefense(VisualElement defenderCard)
    {
        if (defenderCard == null) return;

        if (defenseAssignments.ContainsKey(defenderCard))
            defenseAssignments.Remove(defenderCard);

        if (defenderCard.ClassListContains("defense"))
            defenderCard.RemoveFromClassList("defense");

        selectedCards.Remove(defenderCard);
    }

    private void ClearSelections()
    {
        List<VisualElement> cardsCopy = new List<VisualElement>(selectedCards);
        foreach (VisualElement card in cardsCopy)
        {
            if (card == null) continue;

            if (card.ClassListContains("defense"))
                card.RemoveFromClassList("defense");
        }

        selectedCards.Clear();
        defenseAssignments.Clear();
    }
}
