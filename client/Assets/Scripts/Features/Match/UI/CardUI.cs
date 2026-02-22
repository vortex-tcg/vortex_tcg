using UnityEngine;
using TMPro;
using UnityEngine.UI;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.Services;

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
        public TMP_Text descriptionText;
        
        [SerializeField] private bool faceDown;
        public bool IsFaceDown => faceDown;
        [Header("Attack Phase")] public TMP_Text attackOrderText;

        [Header("Selection")] [SerializeField] private GameObject AttackOutline;
        [SerializeField] private GameObject DefenseOutline;
        [SerializeField] private GameObject AttackOrder;
        [SerializeField] private GameObject SelectedEffect;
        [SerializeField] private float selectedScaleMultiplier = 1.08f;
        private bool isSelected;
        private Vector3 selectionBaseScale;

        void Awake()
        {
            selectionBaseScale = transform.localScale;

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

            if (AttackOutline != null)
                AttackOutline.SetActive(false);

            if (DefenseOutline != null)
                DefenseOutline.SetActive(false);

            if (AttackOrder != null)
                AttackOrder.SetActive(false);
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

            MatchEvents.FireCardClicked(this);
            
            MatchEvents.FireCardSelected(this);
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
            if (hpText != null) hpText.text = hp > 0 ? hp.ToString() : "-";
            if (descriptionText != null) descriptionText.text = description;
        }

        public void ShowAttackOrder(int order)
        {

            if (AttackOrder != null)
            {
                AttackOrder.SetActive(true);
            }

            if (attackOrderText != null)
            {
                attackOrderText.text = order.ToString();
                attackOrderText.enabled = true;
                attackOrderText.ForceMeshUpdate();
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

            if (DefenseOutline != null)
                DefenseOutline.SetActive(false);
        }


        public bool IsSelected => isSelected;

        public void SetSelected(bool selected)
        {
            if (isSelected == selected) return;
            isSelected = selected;

            if (isSelected)
            {
                selectionBaseScale = transform.localScale;
                transform.localScale = selectionBaseScale * selectedScaleMultiplier;

                if (SelectedEffect != null)
                {
                    SelectedEffect.SetActive(true);
                }

                PhaseService phaseService = PhaseService.Instance;
                if (phaseService != null && phaseService.CurrentPhase == GamePhase.ATTACK)
                {
                    CardSlotUI slot = GetComponentInParent<CardSlotUI>();
                    if (slot != null && !slot.isOpponentSlot)
                    {
                        if (AttackOutline != null)
                            AttackOutline.SetActive(true);

                        if (AttackOrder != null)
                            AttackOrder.SetActive(true);
                    }
                }
            }
            else
            {
                transform.localScale = selectionBaseScale;

                if (SelectedEffect != null)
                {
                    SelectedEffect.SetActive(false);
                }

                if (AttackOutline != null)
                    AttackOutline.SetActive(false);

                if (AttackOrder != null)
                    AttackOrder.SetActive(false);
            }
        }

        public void CardIsPlaced() {
            transform.localScale = Vector3.one;

            isSelected = false;

            if (AttackOutline != null)
                AttackOutline.SetActive(false);
            if (AttackOrder != null)
                AttackOrder.SetActive(false);
        }

        public void SetOpponentAttacking(bool active)
        {
            EnsureAttackOutlineRef();
            if (AttackOutline == null) return;
            AttackOutline.SetActive(active);
        }

        private void EnsureAttackOutlineRef()
        {
            if (AttackOutline != null || DefenseOutline == true) return;

            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == "AttackOutline")
                {
                    AttackOutline = t.gameObject;
                    break;
                }
            }
        }

        public void SetDefenseSelected(bool active)
        {
            EnsureDefenseOutlineRef();
            if (DefenseOutline != null)
                DefenseOutline.SetActive(active);
        }

        private void EnsureDefenseOutlineRef()
        {
            if (DefenseOutline != null || AttackOutline == true) return;

            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == "DefenseOutline")
                {
                    DefenseOutline = t.gameObject;
                    break;
                }
            }
        }

        public bool IsAttackingOutlineActive()
        {
            return AttackOutline != null && AttackOutline.activeSelf;
        }
   

        public void SetFaceDown(bool value)
        {
            faceDown = value;
            transform.localRotation = value ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = !value;
        }

    }
}
