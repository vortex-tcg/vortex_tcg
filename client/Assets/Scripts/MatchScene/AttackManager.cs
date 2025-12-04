using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class AttackManager : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private UIDocument boardDocument;

    private VisualElement p1BoardRoot;
    private readonly List<VisualElement> selectedCards = new List<VisualElement>();

    private void OnEnable()
    {
        if (boardDocument == null)
            boardDocument = GetComponent<UIDocument>();

        VisualElement root = boardDocument.rootVisualElement;
        p1BoardRoot = root.Q<VisualElement>("P1BoardCards");

        RegisterAllExistingCards();

        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnEnterAttack += OnEnterAttackPhase;
            PhaseManager.Instance.OnEnterStandBy += OnEndDefensePhase;
        }
    }

    private void OnDisable()
    {
        if (PhaseManager.Instance != null)
        {
            PhaseManager.Instance.OnEnterAttack -= OnEnterAttackPhase;
            PhaseManager.Instance.OnEnterStandBy -= OnEndDefensePhase;
        }
    }

    private void OnEnterAttackPhase()
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
            slot.RegisterCallback<ClickEvent>(ClickEvent =>
            {
                VisualElement card = slot.Q<VisualElement>(className: "small-card");
                if (card != null)
                    ToggleCard(card);
            });
        }
    }

    public void RegisterCard(VisualElement card)
    {
        if (!card.ClassListContains("small-card"))
            return;
        if (card.userData is bool alreadyRegistered && alreadyRegistered)
            return;

        card.userData = true;

        card.RegisterCallback<ClickEvent>(ClickEvent =>
        {
            if (PhaseManager.Instance.CurrentPhase != GamePhase.Attack)
                return;

            if (card.parent == null || card.parent != p1BoardRoot && !p1BoardRoot.Contains(card))
                return;

            ToggleCard(card);
        });
    }

    private void ToggleCard(VisualElement card)
    {
        if (card == null || card.parent == null || card.parent.childCount == 0)
            return;

        if (selectedCards.Contains(card))
            DeselectCard(card);
        else
            SelectCard(card);

        UpdateAttackOrderLabels();
    }

    private void SelectCard(VisualElement card)
    {
        selectedCards.Add(card);
        if (!card.ClassListContains("engaged"))
            card.AddToClassList("engaged");

        Label orderLabel = card.Q<Label>("AttackOrder");
        if (orderLabel != null)
            orderLabel.style.display = DisplayStyle.Flex;
    }

    private void DeselectCard(VisualElement card)
    {
        selectedCards.Remove(card);
        if (card.ClassListContains("engaged"))
            card.RemoveFromClassList("engaged");

        Label orderLabel = card.Q<Label>("AttackOrder");
        if (orderLabel != null)
        {
            orderLabel.text = "";
            orderLabel.style.display = DisplayStyle.None;
        }
    }

    private void UpdateAttackOrderLabels()
    {
        for (int i = 0; i < selectedCards.Count; i++)
        {
            VisualElement card = selectedCards[i];
            Label orderLabel = card.Q<Label>("AttackOrder");
            if (orderLabel != null)
            {
                orderLabel.text = (i + 1).ToString();
                orderLabel.style.display = DisplayStyle.Flex;
            }
        }
    }

    private void ClearSelections()
    {
        foreach (VisualElement card in selectedCards)
        {
            if (card.ClassListContains("engaged"))
                card.RemoveFromClassList("engaged");

            Label orderLabel = card.Q<Label>("AttackOrder");
            if (orderLabel != null)
            {
                orderLabel.text = "";
                orderLabel.style.display = DisplayStyle.None;
            }
        }

        selectedCards.Clear();
    }

}
