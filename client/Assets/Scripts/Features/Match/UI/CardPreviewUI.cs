using UnityEngine;
using UnityEngine.UIElements;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardPreviewUI : MonoBehaviour
    {
        public static CardPreviewUI Instance { get; private set; }

        [SerializeField] private UIDocument uiDocument;

        private VisualElement cardInformationsPreview;
        private Label cardNameLabel;
        private Label cardLoreLabel;
        private Label cardAttackLabel;
        private Label cardAttackPointsLabel;
        private Label cardHealthPointsLabel;
        private Label cardDefensePointsLabel;

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
            VisualElement cardPreviewSide = root.Q<VisualElement>("CardPreviewSide");
            
            if (cardPreviewSide == null)
            {
                Debug.LogError("[CardPreviewUI] CardPreviewSide introuvable.");
                return;
            }

            cardInformationsPreview = cardPreviewSide.Q<VisualElement>("CardInformationsPreview");
            
            if (cardInformationsPreview == null)
            {
                Debug.LogError("[CardPreviewUI] CardInformationsPreview introuvable.");
                return;
            }

            // Récupérer les labels simples
            cardNameLabel = cardInformationsPreview.Q<Label>("CardName");
            cardLoreLabel = cardInformationsPreview.Q<Label>("CardLore");
            cardAttackLabel = cardInformationsPreview.Q<Label>("CardAttack");

            // Récupérer les valeurs de points (2e label de chaque section)
            VisualElement dataSection = cardInformationsPreview.Q<VisualElement>("Data");
            if (dataSection != null)
            {
                // Attack Points - chercher le label avec le texte "20" (2e enfant)
                VisualElement attackPointsContainer = dataSection.Q<VisualElement>("CardAttackPoints");
                if (attackPointsContainer != null)
                {
                    var labels = attackPointsContainer.Query<Label>().ToList();
                    if (labels.Count >= 2)
                        cardAttackPointsLabel = labels[1]; // 2e label
                }

                // Health Points
                VisualElement healthPointsContainer = dataSection.Q<VisualElement>("CardHealthPoints");
                if (healthPointsContainer != null)
                {
                    var labels = healthPointsContainer.Query<Label>().ToList();
                    if (labels.Count >= 2)
                        cardHealthPointsLabel = labels[1]; // 2e label
                }

                // Defense Points
                VisualElement defensePointsContainer = dataSection.Q<VisualElement>("CardDefensePoints");
                if (defensePointsContainer != null)
                {
                    var labels = defensePointsContainer.Query<Label>().ToList();
                    if (labels.Count >= 2)
                        cardDefensePointsLabel = labels[1]; // 2e label
                }
            }

            Debug.Log($"[CardPreviewUI] Initialisation complète - Name:{cardNameLabel!=null}, Lore:{cardLoreLabel!=null}, Attack:{cardAttackLabel!=null}, ATKPoints:{cardAttackPointsLabel!=null}, HPPoints:{cardHealthPointsLabel!=null}, DEFPoints:{cardDefensePointsLabel!=null}");

            HidePreview();
        }

        public void ShowCardPreview(CardUI card)
        {
            if (card == null)
            {
                Debug.LogWarning("[CardPreviewUI] Tentative d'affichage avec une carte null");
                return;
            }

            if (cardInformationsPreview == null)
            {
                Debug.LogWarning("[CardPreviewUI] CardInformationsPreview non initialisé");
                return;
            }

            Debug.Log($"[CardPreviewUI] Affichage de la preview: {card.cardName} (ATK:{card.attack}, HP:{card.hp}, COST:{card.cost})");

            // Mettre à jour les textes
            if (cardNameLabel != null) 
                cardNameLabel.text = card.cardName;
            
            if (cardLoreLabel != null) 
                cardLoreLabel.text = card.description;
            
            if (cardAttackLabel != null) 
                cardAttackLabel.text = card.description; // ou une description de compétence si disponible
            
            if (cardAttackPointsLabel != null) 
                cardAttackPointsLabel.text = card.attack.ToString();
            
            if (cardHealthPointsLabel != null) 
                cardHealthPointsLabel.text = card.hp.ToString();
            
            if (cardDefensePointsLabel != null) 
                cardDefensePointsLabel.text = card.cost.ToString();

            // Afficher le conteneur
            cardInformationsPreview.style.display = DisplayStyle.Flex;
        }

        public void HidePreview()
        {
            if (cardInformationsPreview != null)
                cardInformationsPreview.style.display = DisplayStyle.None;
        }
    }
}
