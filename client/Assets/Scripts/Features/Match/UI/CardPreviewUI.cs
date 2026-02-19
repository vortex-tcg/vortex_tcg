using UnityEngine;
using UnityEngine.UIElements;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardPreviewUI : MonoBehaviour
    {
        public static CardPreviewUI Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;

        private VisualElement cardPreviewContainer;
        private Label nameLabel;
        private Label loreLabel;
        private Label effectNameLabel;
        private Label effectDescLabel;
        private Label atkLabel;
        private Label costLabel;
        private Label defLabel;
        private VisualElement illustration;

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

            if (uiDocument == null)
            {
                Debug.LogError("[CardPreviewUI] UIDocument non assigné.");
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            
            VisualElement leftSide = root.Q<VisualElement>("LeftSide");
            if (leftSide != null)
            {
                VisualElement previewParent = leftSide.Q<VisualElement>("CardPreview");
                if (previewParent != null)
                {
                    cardPreviewContainer = previewParent.Q<VisualElement>("CardPreview");
                }
            }

            if (cardPreviewContainer == null)
            {
                Debug.LogError("[CardPreviewUI] CardPreview introuvable dans la hiérarchie UI.");
                return;
            }

            nameLabel = cardPreviewContainer.Q<Label>("Name");
            loreLabel = cardPreviewContainer.Q<Label>("Lore");
            effectNameLabel = cardPreviewContainer.Q<Label>("EffectName");
            effectDescLabel = cardPreviewContainer.Q<Label>("EffectDesc");
            atkLabel = cardPreviewContainer.Q<Label>("ATK");
            costLabel = cardPreviewContainer.Q<Label>("COST");
            defLabel = cardPreviewContainer.Q<Label>("DEF");
            illustration = cardPreviewContainer.Q<VisualElement>("Illustration");

            Debug.Log($"[CardPreviewUI] Labels found - Name:{nameLabel!=null}, ATK:{atkLabel!=null}, DEF:{defLabel!=null}");

            HidePreview();
        }

        public void ShowCardPreview(CardUI card)
        {
            if (card == null || cardPreviewContainer == null)
            {
                Debug.LogWarning("[CardPreviewUI] Cannot show preview - card or container null");
                return;
            }

            Debug.Log($"[CardPreviewUI] Showing preview for: {card.cardName}");

            if (nameLabel != null) nameLabel.text = card.cardName;
            if (loreLabel != null) loreLabel.text = card.description;
            if (atkLabel != null) atkLabel.text = card.attack.ToString();
            if (costLabel != null) costLabel.text = card.cost.ToString();
            if (defLabel != null) defLabel.text = card.hp.ToString();

            cardPreviewContainer.style.display = DisplayStyle.Flex;
        }

        public void HidePreview()
        {
            if (cardPreviewContainer != null)
                cardPreviewContainer.style.display = DisplayStyle.None;
        }
    }
}
