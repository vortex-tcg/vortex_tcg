using UnityEngine;
using TMPro;
using UnityEngine.UI;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.Services;
using VortexTCG.Scripts.Features.Match.UI;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardUI : MonoBehaviour
    {
        [Header("Data")] public string cardId;
        
        public string cardName;
        public int hp;
        public int attack;
        public int cost;
        public Transform VisualRoot;
        public Transform UIRoot;

        [TextArea(3, 6)] public string description;
        public string imageUrl;

        [Header("UI")] public TMP_Text nameText;
        public TMP_Text costText;
        public TMP_Text atkText;
        public TMP_Text hpText;
        public TMP_Text currentHpText;
        public TMP_Text descriptionText;
        [SerializeField] private SpriteRenderer costColor;
        [Header("Cost Colors")]
        [SerializeField] private Color costGreen = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color costBlue = new Color(0.2f, 0.4f, 0.9f, 1f);
        [SerializeField] private Color costOrange = new Color(1f, 0.55f, 0.1f, 1f);
        [SerializeField] private Color costRed = new Color(0.9f, 0.15f, 0.15f, 1f);
        [SerializeField] private Color costViolet = new Color(0.6f, 0.2f, 0.8f, 1f);
        
        [SerializeField] private bool faceDown;
        public bool IsFaceDown => faceDown;

        [Header("Sleepy")]
        [Tooltip("Optional gameobject to enable when the card is sleepy (first turn)")]
        [SerializeField] private GameObject sleepyEffect;
        private bool isSleepy;
        public bool IsSleepy => isSleepy;

        [Header("Attack Phase")] public TMP_Text attackOrderText;

        [Header("Selection")] [SerializeField] private GameObject AttackState;
        [SerializeField] private GameObject DefenseState;
        [SerializeField] private GameObject AttackOrder;
        [SerializeField] private float selectedScaleMultiplier = 1.08f;
        private bool isSelected;
        private Vector3 selectionBaseScale;
        private bool hasAttackedThisPhase = false;

        void Awake()
        {
            selectionBaseScale = transform.localScale;

            if (currentHpText == null)
                TryResolveCurrentHpText();

            if (costColor == null)
                TryResolveCostColor();

            Collider col = GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                bc.size = new Vector3(2f, 3f, 0.1f);
            }

            if (AttackOrder != null && attackOrderText == null)
            {
                attackOrderText = AttackOrder.GetComponentInChildren<TMP_Text>();
            }

            if (attackOrderText != null)
                attackOrderText.enabled = false;

            if (AttackOrder != null)
                AttackOrder.SetActive(false);

            UpdateCostColor();

            // if the sleep manager is currently active (first turn), start sleepy
            if (SleepManager.IsSleeping)
            {
                SetSleepy(true);
            }

            UpdateCurrentHpDisplay();
            UpdateCurrentHpVisibility();
        }

        private void OnValidate()
        {
            if (costColor == null)
                TryResolveCostColor();

            if (currentHpText == null)
                TryResolveCurrentHpText();

            UpdateCostColor();
            UpdateCurrentHpDisplay();
            UpdateCurrentHpVisibility();
        }

        private void OnTransformParentChanged()
        {
            UpdateCurrentHpVisibility();
        }

        void OnMouseEnter()
        {
            if (faceDown) return;

            CardSlotUI slot = GetComponentInParent<CardSlotUI>();

            if (CardPreviewUI.Instance != null)
            {
                CardPreviewUI.Instance.ShowCardPreview(this);
            }
        }

        void OnMouseExit()
        {
            if (CardPreviewUI.Instance != null)
            {
                CardPreviewUI.Instance.HidePreview();
            }
        }

        void OnMouseDown()
        {
            if (string.IsNullOrEmpty(cardName) || string.IsNullOrEmpty(cardId))
            {
                return;
            }
            
            if (faceDown) return;

            if (isSleepy)
            {
                Debug.Log($"[CardUI] '{cardName}' is sleepy; click ignored");
                return;
            }

            Debug.Log($"[CardUI] OnMouseDown on '{cardName}' (ID: {cardId}) - Current Phase: {(PhaseService.Instance != null ? PhaseService.Instance.CurrentPhase.ToString() : "NULL")}");

            MatchEvents.FireCardClicked(this);
            

            if (PhaseService.Instance == null || PhaseService.Instance.CurrentPhase != GamePhase.DEFENSE)
            {
                MatchEvents.FireCardSelected(this);
            }
        }

        public void ApplyDTO(string id, string name, int hp, int attack, int cost, string desc, string imgUrl)
        {
            cardId = id;
            cardName = name;
            this.hp = hp;
            this.attack = attack;
            this.cost = cost;
            description = desc;
            imageUrl = imgUrl;
            RefreshUI();
        }

        public void RefreshUI()
        {

            if (nameText != null) nameText.text = cardName;
            if (costText != null) costText.text = cost.ToString();
            if (atkText != null) atkText.text = attack > 0 ? attack.ToString() : "-";
            if (hpText != null) hpText.text = Mathf.Max(0, hp).ToString();
            UpdateCurrentHpDisplay();
            if (descriptionText != null) descriptionText.text = description;
            UpdateCostColor();
            UpdateCurrentHpVisibility();
        }

        public void RefreshCurrentHpVisibility()
        {
            UpdateCurrentHpVisibility();
        }

        private void UpdateCurrentHpDisplay()
        {
            if (currentHpText == null)
                return;

            currentHpText.text = Mathf.Max(0, hp).ToString();
        }

        private void UpdateCurrentHpVisibility()
        {
            if (currentHpText == null)
                return;

            CardSlotUI slot = GetComponentInParent<CardSlotUI>();
            bool isOnPlayerBoard = AttackUI.Instance != null && AttackUI.Instance.IsCardOnP1Board(this);
            bool isOnOpponentBoard = OpponentBoardUI.Instance != null && OpponentBoardUI.Instance.IsCardOnOpponentBoard(this);

            bool isLikelyBoardByName = slot != null &&
                                     (slot.name.Contains("BoardSlot") || slot.name.Contains("P1Board") || slot.name.Contains("P2Board"));

            bool isOnBoardSlot = isOnPlayerBoard || isOnOpponentBoard || isLikelyBoardByName;
            currentHpText.gameObject.SetActive(isOnBoardSlot);
        }

        private void UpdateCostColor()
        {
            if (costColor == null)
                return;

            int clampedCost = Mathf.Clamp(cost, 0, 10);
            Color target = clampedCost switch
            {
                0 or 1 or 2 => costGreen,
                3 or 4 => costBlue,
                5 or 6 => costOrange,
                7 or 8 => costRed,
                _ => costViolet
            };

            Color current = costColor.color;
            costColor.color = new Color(target.r, target.g, target.b, current.a);
        }

        private void TryResolveCostColor()
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                if (sr != null && (sr.name == "costColor" || sr.name == "CostColor"))
                {
                    costColor = sr;
                    return;
                }
            }
        }

        private void TryResolveCurrentHpText()
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text txt = texts[i];
                if (txt != null && (txt.name == "CurrentHP" || txt.name == "currentHP" || txt.name == "CurrentHp"))
                {
                    currentHpText = txt;
                    return;
                }
            }
        }

        public void ShowAttackOrder(int order)
        {
            if (attackOrderText != null)
            {
                attackOrderText.text = order.ToString();
                attackOrderText.enabled = true;
            }

            if (AttackOrder != null)
                AttackOrder.SetActive(true);
        }

        public void ClearAttackOrder()
        {
            if (attackOrderText != null)
            {
                attackOrderText.text = "";
                attackOrderText.enabled = false;
            }

            if (AttackOrder != null)
                AttackOrder.SetActive(false);
        }


        public bool IsSelected => isSelected;
        public bool HasAttackedThisPhase => hasAttackedThisPhase;

        public void SetAttackedThisPhase(bool hasAttacked)
        {
            hasAttackedThisPhase = hasAttacked;
        }

        public void ResetAttackState()
        {
            hasAttackedThisPhase = false;
        }

        public void SetSelected(bool selected)
        {
            if (isSelected == selected) return;
            isSelected = selected;

            if (isSelected)
            {
                selectionBaseScale = transform.localScale;
                transform.localScale = selectionBaseScale * selectedScaleMultiplier;


                PhaseService phaseService = PhaseService.Instance;
                if (phaseService != null && phaseService.CurrentPhase == GamePhase.ATTACK)
                {
                    CardSlotUI slot = GetComponentInParent<CardSlotUI>();
                    if (slot != null && !slot.isOpponentSlot)
                    {
                        if (AttackOrder != null)
                            AttackOrder.SetActive(true);
                    }
                }
            }
            else
            {
                transform.localScale = selectionBaseScale;

                if (AttackOrder != null)
                    AttackOrder.SetActive(false);
            }

            AttackUI.Instance?.UpdateAttackStateForSelection(this, isSelected);
        }

        public void CardIsPlaced() {
            transform.localScale = Vector3.one;

            isSelected = false;

            if (AttackOrder != null)
                AttackOrder.SetActive(false);

            AttackUI.Instance?.UpdateAttackStateForSelection(this, false);
        }

        public void SetOpponentAttacking(bool active)
        {
            GameObject attackState = GetAttackState();
            if (attackState != null)
            {
                attackState.SetActive(active);
            }
        }

        public GameObject GetAttackState()
        {
            if (AttackState == null)
            {
                AttackState = FindOutlineByName("AttackState");
                Debug.Log($"[CardUI] GetAttackState for card '{cardName}': {(AttackState != null ? "FOUND" : "NOT FOUND")}");
            }

            return AttackState;
        }

        public void SetDefenseSelected(bool active)
        {
            DefenseUI.Instance?.SetDefenseState(this, active);
        }

        public GameObject GetDefenseState()
        {
            if (DefenseState == null)
                DefenseState = FindOutlineByName("DefenseState");

            return DefenseState;
        }

        private GameObject FindOutlineByName(string name)
        {
            Debug.Log($"[CardUI] FindOutlineByName searching for '{name}' in card '{cardName}' (ID: {cardId})");
            
            Transform[] children = GetComponentsInChildren<Transform>(true);
            Debug.Log($"[CardUI] Found {children.Length} children (including self and inactive)");
            
            foreach (Transform t in children)
            {
                if (t != null)
                {
                    Debug.Log($"[CardUI]   - Child: '{t.name}' (active: {t.gameObject.activeSelf})");
                    if (t.name == name)
                    {
                        Debug.Log($"[CardUI] ✓ MATCH FOUND: '{name}' on card '{cardName}'");
                        return t.gameObject;
                    }
                }
            }

            Debug.LogWarning($"[CardUI] ✗ '{name}' NOT FOUND on card '{cardName}' (ID: {cardId})");
            return null;
        }

        public bool IsAttackingOutlineActive()
        {
            return AttackState != null && AttackState.activeSelf;
        }

        public bool IsDefenseSelected()
        {
            return DefenseState != null && DefenseState.activeSelf;
        }
   

        public void SetFaceDown(bool value)
        {
            faceDown = value;
            transform.localRotation = value ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = !value;
        }

        /// <summary>
        /// Marks the card as sleepy (cannot be used for attack/defense) and toggles the associated effect.
        /// </summary>
        public void SetSleepy(bool sleepy)
        {
            if (isSleepy == sleepy) return;
            isSleepy = sleepy;

            if (sleepyEffect != null)
                sleepyEffect.SetActive(sleepy);

            // optional: disable collider to prevent any interaction
            // Collider col = GetComponent<Collider>();
            // if (col != null) col.enabled = !sleepy;

            Debug.Log($"[CardUI] SetSleepy('{cardName}' ID:{cardId}) -> {sleepy}");
        }

    }
}
