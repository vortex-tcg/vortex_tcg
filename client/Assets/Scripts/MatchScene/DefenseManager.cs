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

        Debug.Log("DefenseManager subscribed to phase events");
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
        Debug.Log("=== DEFENSE PHASE STARTED ===");
        ClearSelections();
    }

    private void OnEndDefensePhase()
    {
        Debug.Log("=== STANDBY PHASE - CLEARING DEFENSE ===");
        ClearSelections();
    }

    private void RegisterAllExistingCards()
    {
        var allSlots = p1BoardRoot.Query<VisualElement>(className: "P1Slot").ToList();
        foreach (var slot in allSlots)
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
        {
            Debug.LogWarning("La carte défensive doit être sur le board !");
            return;
        }

        if (!targetCard.ClassListContains("engaged"))
        {
            Debug.LogWarning("La cible doit être une carte en mode attaque !");
            return;
        }

        VisualElement existingDefender = defenseAssignments.FirstOrDefault(kvp => kvp.Value == targetCard).Key;
        if (existingDefender != null)
        {
            Debug.LogWarning($"Une carte défend déjà contre {targetCard.name} !");
            return;
        }

        ClearDefense(defenderCard);

        defenderCard.AddToClassList("defense");
        defenseAssignments[defenderCard] = targetCard;

        if (!selectedCards.Contains(defenderCard))
            selectedCards.Add(defenderCard);

        Debug.Log($"{defenderCard.name} est maintenant en mode défense contre {targetCard.name}");
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

        Debug.Log($"{card.name} est maintenant en mode défense");
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
        Debug.Log($"[DefenseManager] Clearing {selectedCards.Count} defense cards");

        foreach (var card in selectedCards.ToList())
        {
            if (card == null)
            {
                Debug.LogWarning("[DefenseManager] Found null card in selectedCards");
                continue;
            }

            Debug.Log($"[DefenseManager] Removing 'defense' class from {card.name}");

            if (card.ClassListContains("defense"))
                card.RemoveFromClassList("defense");
        }

        selectedCards.Clear();
        defenseAssignments.Clear();

        Debug.Log("[DefenseManager] Defense selections cleared");
    }
}