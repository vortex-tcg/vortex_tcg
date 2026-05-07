using UnityEngine;
using UnityEngine.UIElements;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardPreviewUI : MonoBehaviour
    {
        public static CardPreviewUI Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;

        [Header("Cost Colors")]
        [SerializeField] private Color costGreen = new Color(0.2f, 0.8f, 0.2f, 1f);
        [SerializeField] private Color costBlue = new Color(0.2f, 0.4f, 0.9f, 1f);
        [SerializeField] private Color costOrange = new Color(1f, 0.55f, 0.1f, 1f);
        [SerializeField] private Color costRed = new Color(0.9f, 0.15f, 0.15f, 1f);
        [SerializeField] private Color costViolet = new Color(0.6f, 0.2f, 0.8f, 1f);

        private VisualElement cardInformationsPreview;
        private VisualElement dataSection;
        private VisualElement costPointsContainer;

        private Label cardNameLabel;
        private Label cardLoreLabel;
        private Label attackPoints;
        private Label healthPoints;
        private Label defensePoints;
        private Label costPoints;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            VisualElement root = uiDocument.rootVisualElement;

            cardInformationsPreview = root.Q<VisualElement>("CardInformationsPreview");
            cardNameLabel = cardInformationsPreview.Q<Label>("CardName");
            cardLoreLabel = cardInformationsPreview.Q<Label>("CardLore");
            dataSection = cardInformationsPreview.Q<VisualElement>("Data");

            VisualElement attackPointsContainer = dataSection?.Q<VisualElement>("CardAttackPoints");
            VisualElement healthPointsContainer = dataSection?.Q<VisualElement>("CardHealthPoints");
            VisualElement defensePointsContainer = dataSection?.Q<VisualElement>("CardDefensePoints");

            // ✅ IMPORTANT : pas de "VisualElement" devant → on stocke dans la variable de classe
            costPointsContainer = dataSection?.Q<VisualElement>("CardCostPoints");

            attackPoints = attackPointsContainer?.Q<Label>("ATKPoints");
            healthPoints = healthPointsContainer?.Q<Label>("HPPoints");
            defensePoints = defensePointsContainer?.Q<Label>("DEFPoints");
            costPoints = costPointsContainer?.Q<Label>("CostPoints");

            HidePreview();
        }

        public void ShowCardPreview(CardUI card)
        {
            if (string.IsNullOrEmpty(card.cardName))
            {
                HidePreview();
                return;
            }

            if (cardInformationsPreview != null)
                cardInformationsPreview.style.display = DisplayStyle.Flex;

            if (cardNameLabel != null)
            {
                cardNameLabel.style.display = DisplayStyle.Flex;
                cardNameLabel.text = card.cardName;
            }

            if (cardLoreLabel != null)
            {
                cardLoreLabel.style.display = DisplayStyle.Flex;
                cardLoreLabel.text = card.description;
            }

            if (dataSection != null)
            {
                dataSection.style.display = DisplayStyle.Flex;

                if (attackPoints != null)
                    attackPoints.text = card.attack.ToString();

                if (healthPoints != null)
                    healthPoints.text = card.hp.ToString();

                if (costPoints != null)
                {
                    costPoints.text = card.cost.ToString();
                    UpdateCostColor(card.cost); // ✅ appel ici
                }
            }
        }

        public void HidePreview()
        {
            if (cardInformationsPreview != null)
                cardInformationsPreview.style.display = DisplayStyle.None;
        }

        private void UpdateCostColor(int cost)
        {
            if (costPointsContainer == null)
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

            // 🎨 UI Toolkit → on change le background
            costPointsContainer.style.backgroundColor = target;
        }
    }
}