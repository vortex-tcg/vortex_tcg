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
                Debug.LogWarning($"[Card] {gameObject.name} n'a PAS de Collider ! Ajout d'un BoxCollider.");
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                bc.size = new Vector3(2f, 3f, 0.1f);
            }
            else
            {
                Debug.Log($"[Card] {gameObject.name} a un Collider: {col.GetType().Name}, enabled={col.enabled}, isTrigger={col.isTrigger}");
                if (col is BoxCollider box)
                {
                    Debug.Log($"[Card] BoxCollider size: {box.size}, center: {box.center}");
                }
            }

            Camera cam = Camera.main;
            if (cam != null)
            {
                Debug.Log($"[Card] Camera.main trouvée: {cam.name}, tag={cam.tag}");
            }
            else
            {
                Debug.LogError("[Card] Camera.main est NULL ! OnMouseDown ne fonctionnera pas.");
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
            if (slot != null && slot.isOpponentSlot) return;

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
            // Prevent ANY interaction if card has no valid data
            if (string.IsNullOrEmpty(cardName) || string.IsNullOrEmpty(cardId))
            {
                Debug.LogWarning($"[Card] ❌ BLOCKING click on invalid card - GameObject={gameObject.name}, cardName='{cardName}', cardId='{cardId}'");
                return;
            }
            
            Debug.Log($"[Card] OnMouseDown sur '{cardName}' (ID={cardId}), faceDown={faceDown}");
            
            if (faceDown) return;

            // Déclencher événement générique de click
            MatchEvents.FireCardClicked(this);
            
            // Aussi déclencher sélection pour la main (le service/UI décidera)
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
            Debug.Log($"[Card] RefreshUI nameText={(nameText!=null)} costText={(costText!=null)} atkText={(atkText!=null)} hpText={(hpText!=null)} descText={(descriptionText!=null)}");

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
            else
            {
                Debug.LogError($"[Card] ShowAttackOrder({order}) - attackOrderText est NULL!");
            }

            Debug.Log(
                $"[Card] ShowAttackOrder({order}) - AttackOrder={AttackOrder?.activeSelf}, AttackOrder.activeInHierarchy={AttackOrder?.activeInHierarchy}");
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

                // Activer le SelectedEffect
                if (SelectedEffect != null)
                {
                    SelectedEffect.SetActive(true);
                    Debug.Log($"[Card] SelectedEffect activated for '{cardName}'");
                }

                // Afficher visuels d'attaque si en phase ATTACK et sur plateau joueur
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

                // Désactiver le SelectedEffect
                if (SelectedEffect != null)
                {
                    SelectedEffect.SetActive(false);
                    Debug.Log($"[Card] SelectedEffect deactivated for '{cardName}'");
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
            Debug.Log("Je try d'attack !!!!");
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
            // ✅ Rotate around Z axis to flip the card
            transform.localRotation = value ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = !value;
        }

    }
}
