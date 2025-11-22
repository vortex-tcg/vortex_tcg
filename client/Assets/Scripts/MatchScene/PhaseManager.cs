using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum GamePhase
{
    StandBy,
    Attack,
    Defense
}

public class PhaseManager : MonoBehaviour
{
    public static PhaseManager Instance { get; private set; }

    [Header("UI Toolkit")]
    [SerializeField] private UIDocument uiDoc;

    private Button phaseButton;
    private VisualElement standbyIcon;
    private VisualElement attackIcon;
    private VisualElement defenseIcon;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.StandBy;

    public event Action OnEnterStandBy;
    public event Action OnEnterAttack;
    public event Action OnEnterDefense;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (uiDoc == null)
            uiDoc = GetComponent<UIDocument>();

        var root = uiDoc.rootVisualElement;

        phaseButton = root.Q<Button>("EndPhaseButton");
        standbyIcon = root.Q<VisualElement>("StandBy");
        attackIcon = root.Q<VisualElement>("Attack");
        defenseIcon = root.Q<VisualElement>("Defense");

        if (phaseButton != null)
            phaseButton.clicked += NextPhase;

        UpdateIcons(CurrentPhase);
    }

    private void NextPhase()
    {
        CurrentPhase = CurrentPhase switch
        {
            GamePhase.StandBy => GamePhase.Attack,
            GamePhase.Attack => GamePhase.Defense,
            _ => GamePhase.StandBy
        };

        switch (CurrentPhase)
        {
            case GamePhase.StandBy: OnEnterStandBy?.Invoke(); break;
            case GamePhase.Attack: OnEnterAttack?.Invoke(); break;
            case GamePhase.Defense: OnEnterDefense?.Invoke(); break;
        }

        UpdateIcons(CurrentPhase);
    }

    private void UpdateIcons(GamePhase phase)
    {
        SetHighlight(standbyIcon, phase == GamePhase.StandBy);
        SetHighlight(attackIcon, phase == GamePhase.Attack);
        SetHighlight(defenseIcon, phase == GamePhase.Defense);
    }

    private void SetHighlight(VisualElement icon, bool active)
    {
        if (icon == null) return;
        icon.style.opacity = active ? 1f : 0.3f;
    }
}
