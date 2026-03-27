using UnityEngine;
using UnityEngine.UIElements;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardPreviewUI : MonoBehaviour
    {
        public static CardPreviewUI Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;

        private VisualElement cardInformationsPreview;
        private VisualElement dataSection;
        private Label cardNameLabel;
        private Label cardLoreLabel;
        private Label cardAttackLabel;
        private Label attackPoints;
        private Label healthPoints;
        private Label defensePoints;

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
            cardAttackLabel = cardInformationsPreview.Q<Label>("CardAttack");
            dataSection = cardInformationsPreview.Q<VisualElement>("Data");
            
            VisualElement attackPointsContainer = dataSection?.Q<VisualElement>("CardAttackPoints");
            VisualElement healthPointsContainer = dataSection?.Q<VisualElement>("CardHealthPoints");
            VisualElement defensePointsContainer = dataSection?.Q<VisualElement>("CardDefensePoints");
            
            attackPoints = attackPointsContainer?.Q<Label>("ATKPoints");
            healthPoints = healthPointsContainer?.Q<Label>("HPPoints");
            defensePoints = defensePointsContainer?.Q<Label>("DEFPoints");

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
            
            if (cardAttackLabel != null) 
            {
                cardAttackLabel.style.display = DisplayStyle.Flex;
                cardAttackLabel.text = card.attack.ToString();
            }
            
            if (dataSection != null)
            {
                dataSection.style.display = DisplayStyle.Flex;
                
                if (attackPoints != null) 
                    attackPoints.text = card.attack.ToString();
                
                if (healthPoints != null) 
                    healthPoints.text = card.hp.ToString();
            
            }
        }

        public void HidePreview()
        {
            if (cardInformationsPreview != null)
                cardInformationsPreview.style.display = DisplayStyle.None;
        }
    }
}
