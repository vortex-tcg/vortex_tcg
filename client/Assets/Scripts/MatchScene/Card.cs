using UnityEngine;
using TMPro;
using UnityEngine.UI;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class Card : MonoBehaviour
    {
        [Header("Data")] public string cardId;
        public string cardName;
        public int hp;
        public int attack;
        public int cost;
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
        [SerializeField] private float selectedScaleMultiplier = 1.08f;
        private bool isSelected;
        private Vector3 selectionBaseScale;

        void Awake()
        {
            selectionBaseScale = transform.localScale;

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

        void OnMouseDown()
        {
            if (faceDown) return;
            if (AttackOutline != null && AttackOutline.activeSelf)
            {
                if (PhaseManager.Instance == null ||
                    (PhaseManager.Instance.CurrentPhase != GamePhase.ATTACK &&
                     PhaseManager.Instance.CurrentPhase != GamePhase.DEFENSE))
                {
                    return;
                }
            }

            CardSlot slot = GetComponentInParent<CardSlot>();
            if (slot != null && PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase == GamePhase.ATTACK)
            {
                if (AttackManager.Instance != null && AttackManager.Instance.IsP1BoardSlot(slot))
                {
                    AttackManager.Instance.HandleCardClicked(this);
                    return;
                }
            }

            if (PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase == GamePhase.DEFENSE)
            {
                if (DefenseManager.Instance != null)
                {
                    DefenseManager.Instance.HandleCardClicked(this);
                    return;
                }
            }

            HandManager.Instance.SelectCard(this);
        }


        void MockData()
        {
            if (string.IsNullOrEmpty(cardId))
                cardId = System.Guid.NewGuid().ToString();

            int randomCard = Random.Range(0, 4);
            switch (randomCard)
            {
                case 0:
                    cardName = "Guerrier";
                    hp = 10;
                    attack = 5;
                    cost = 3;
                    description = "Un guerrier courageux et résistant.";
                    imageUrl = "https://example.com/images/guerrier.png";
                    break;
                case 1:
                    cardName = "Mage";
                    hp = 6;
                    attack = 8;
                    cost = 4;
                    description = "Un mage puissant maîtrisant les sorts.";
                    imageUrl = "https://example.com/images/mage.png";
                    break;
                case 2:
                    cardName = "Soigneur";
                    hp = 5;
                    attack = 2;
                    cost = 2;
                    description = "Soigne les alliés pendant le combat.";
                    imageUrl = "https://example.com/images/soigneur.png";
                    break;
                case 3:
                    cardName = "Archer";
                    hp = 7;
                    attack = 6;
                    cost = 3;
                    description = "Un archer agile et précis.";
                    imageUrl = "https://example.com/images/archer.png";
                    break;
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


                RectTransform rectTransform = attackOrderText.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

                    rectTransform.sizeDelta = new Vector2(150, -200);


                    Debug.Log($"[Card] ShowAttackOrder({order}) - RectTransform resizé à {rectTransform.sizeDelta}");
                }

                Debug.Log(
                    $"[Card] ShowAttackOrder({order}) - text='{attackOrderText.text}', enabled={attackOrderText.enabled}, fontSize={attackOrderText.fontSize}");
            }
            else
            {
                Debug.LogError($"[Card] ShowAttackOrder({order}) - attackOrderText est NULL!");
            }

            // Debug pour vérifier
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


        public void SetSelected(bool selected)
        {
            if (isSelected == selected) return;
            isSelected = selected;

            if (isSelected)
            {
                selectionBaseScale = transform.localScale;
                transform.localScale = selectionBaseScale * selectedScaleMultiplier;

                bool canShowAttackVisuals = false;
                if (PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase == GamePhase.ATTACK)
                {
                    CardSlot slot = GetComponentInParent<CardSlot>();
                    if (slot != null && AttackManager.Instance != null && AttackManager.Instance.IsP1BoardSlot(slot))
                    {
                        canShowAttackVisuals = true;
                    }
                }

                if (canShowAttackVisuals)
                {
                    if (AttackOutline != null)
                        AttackOutline.SetActive(true);

                    if (AttackOrder != null)
                        AttackOrder.SetActive(true);
                }
            }
            else
            {
                transform.localScale = selectionBaseScale;

                if (AttackOutline != null)
                    AttackOutline.SetActive(false);

                if (AttackOrder != null)
                    AttackOrder.SetActive(false);
            }
        }

        public void SetOpponentAttacking(bool active)
        {
            if (AttackOutline == null) return;

            AttackOutline.SetActive(active);

            Renderer renderer = AttackOutline.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = active ? Color.red : Color.white;
            }
        }

        public void SetDefenseSelected(bool active)
        {
            if (DefenseOutline != null)
                DefenseOutline.SetActive(active);
        }

        public bool IsAttackingOutlineActive()
        {
            return AttackOutline != null && AttackOutline.activeSelf;
        }
   

        public void SetFaceDown(bool value)
        {
            faceDown = value;
            transform.localRotation = value ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = !value;
        }

    }
}
