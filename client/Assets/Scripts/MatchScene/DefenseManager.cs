using System.Collections.Generic;
using UnityEngine;

// Runtime DefenseManager: selects a P1 defender during Defense phase, then assigns it to an attacking P2 card.
public class DefenseManager : MonoBehaviour
{
    public static DefenseManager Instance { get; private set; }

    [Header("Board Slots")]
    [SerializeField] private List<CardSlot> P1BoardSlots = new List<CardSlot>();
    [SerializeField] private List<CardSlot> P2BoardSlots = new List<CardSlot>();

    private Card currentDefender;
    private readonly Dictionary<Card, Card> defenseAssignments = new Dictionary<Card, Card>(); // defender -> attacker

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnEnterDefense += OnEnterDefense;
            PhaseManager.Instance.OnEnterStandBy += OnExitDefense;
            PhaseManager.Instance.OnEnterAttack += OnExitDefense;
        }
    }

    private void OnDestroy()
    {
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnEnterDefense -= OnEnterDefense;
            PhaseManager.Instance.OnEnterStandBy -= OnExitDefense;
            PhaseManager.Instance.OnEnterAttack -= OnExitDefense;
        }
    }

    private void OnEnterDefense()
    {
        ClearAllDefense();
    }

    private void OnExitDefense()
    {
        ClearAllDefense();
    }

    public bool IsP1BoardSlot(CardSlot slot)
    {
        return slot != null && P1BoardSlots.Contains(slot);
    }

    public bool IsP2BoardSlot(CardSlot slot)
    {
        return slot != null && P2BoardSlots.Contains(slot);
    }

    public void HandleCardClicked(Card card)
    {
        if (card == null) return;
        if (PhaseManager.Instance == null || PhaseManager.Instance.CurrentPhase != GamePhase.Defense) return;

        var slot = card.GetComponentInParent<CardSlot>();
        if (slot == null) return;

        if (IsP1BoardSlot(slot))
        {
            SelectDefender(card);
        }
        else if (IsP2BoardSlot(slot))
        {
            TryAssignDefense(card);
        }
    }

    private void SelectDefender(Card defender)
    {
        if (currentDefender == defender) return;

        // Ne pas désactiver l'ancien défenseur : il reste en mode défense visible
        currentDefender = defender;
        currentDefender.SetDefenseSelected(true);

        Debug.Log($"[Defense] Defender selected (keeps previous in defense): {CardLabel(defender)}");
    }

    private void TryAssignDefense(Card targetAttacker)
    {
        if (currentDefender == null)
        {
            Debug.LogWarning("[Defense] No defender selected. Click a P1 card first.");
            return;
        }

        // Only allow assigning to attacking enemy cards (outline active)
        if (!targetAttacker.IsAttackingOutlineActive())
        {
            Debug.Log("[Defense] Target is not marked as attacking; assignment ignored.");
            return;
        }

        // Remove previous assignment for this defender if any
        if (defenseAssignments.ContainsKey(currentDefender))
        {
            Debug.Log($"[Defense] Replacing previous assignment for {CardLabel(currentDefender)}");
            defenseAssignments.Remove(currentDefender);
        }

        defenseAssignments[currentDefender] = targetAttacker;

        Debug.Log($"[Defense] Assigned {CardLabel(currentDefender)} to defend against {CardLabel(targetAttacker)}");
    }

    private void ClearAllDefense()
    {
        if (currentDefender != null)
        {
            currentDefender.SetDefenseSelected(false);
            currentDefender = null;
        }

        foreach (var kvp in defenseAssignments)
        {
            if (kvp.Key != null)
                kvp.Key.SetDefenseSelected(false);
        }

        defenseAssignments.Clear();

        Debug.Log("[Defense] Cleared all defense selections/assignments");
    }

    private string CardLabel(Card c)
    {
        if (c == null) return "null";
        return string.IsNullOrEmpty(c.cardId) ? c.cardName : $"{c.cardName} ({c.cardId})";
    }
}
