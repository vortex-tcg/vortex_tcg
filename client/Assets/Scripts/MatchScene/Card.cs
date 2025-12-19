using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Data")]
    public string cardId;
    public string cardName;
    public int hp;
    public int attack;
    public int cost;
    [TextArea(3, 6)]
    public string description;
    public string imageUrl; // URL de l'image pour futur usage

    [Header("UI")]
    public TMP_Text nameText;
    public TMP_Text costText;
    public TMP_Text atkText;
    public TMP_Text hpText;
    public TMP_Text descriptionText;
    
    [Header("Attack Phase")]
    public TMP_Text attackOrderText;

    [Header("Selection")]
    [SerializeField] private GameObject AttackOutline;
    [SerializeField] private GameObject DefenseOutline;
    [SerializeField] private GameObject AttackOrder;
    [SerializeField] private float selectedScaleMultiplier = 1.08f;
    private bool isSelected;
    private Vector3 selectionBaseScale;

    void Awake()
    {
        selectionBaseScale = transform.localScale;
        MockData();
        RefreshUI();
        
        // Si AttackOrder GameObject est assigné, chercher le TMP_Text dedans
        if (AttackOrder != null && attackOrderText == null)
        {
            attackOrderText = AttackOrder.GetComponentInChildren<TMP_Text>();
            if (attackOrderText != null)
            {
                Debug.Log($"[Card] attackOrderText trouvé automatiquement dans {AttackOrder.name}");
            }
            else
            {
                Debug.LogWarning($"[Card] Aucun TMP_Text trouvé dans {AttackOrder.name}");
            }
        }
        
        // Masquer le texte d'ordre d'attaque et l'outline au démarrage
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
        Debug.Log("Carte cliquée !");

        // Bloquer l'interaction si l'outline d'attaque est actif, sauf en phases Attack ou Defense (Defense doit pouvoir cibler les attaquants)
        if (AttackOutline != null && AttackOutline.activeSelf)
        {
            if (PhaseManager.Instance == null ||
                (PhaseManager.Instance.CurrentPhase != GamePhase.Attack && PhaseManager.Instance.CurrentPhase != GamePhase.Defense))
            {
                Debug.Log("Carte verrouillée en mode attaque - interaction bloquée hors phase Attack/Defense");
                return;
            }
        }

        // Si la carte est sur le board P1 et qu'on est en phase d'attaque,
        // déléguer au AttackManager pour la sélection d'ordre d'attaque.
        var slot = GetComponentInParent<CardSlot>();
        if (slot != null && PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase == GamePhase.Attack)
        {
            if (AttackManager.Instance != null && AttackManager.Instance.IsP1BoardSlot(slot))
            {
                AttackManager.Instance.HandleCardClicked(this);
                return;
            }
        }

        // Phase Defense : sélection ou assignment via DefenseManager
        if (PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase == GamePhase.Defense)
        {
            if (DefenseManager.Instance != null)
            {
                DefenseManager.Instance.HandleCardClicked(this);
                return;
            }
        }

        // Sinon, comportement par défaut (sélection depuis la main, etc.)
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
                cardName = "Guerrier"; hp = 10; attack = 5; cost = 3; description = "Un guerrier courageux et résistant."; imageUrl = "https://example.com/images/guerrier.png"; break;
            case 1:
                cardName = "Mage"; hp = 6; attack = 8; cost = 4; description = "Un mage puissant maîtrisant les sorts."; imageUrl = "https://example.com/images/mage.png"; break;
            case 2:
                cardName = "Soigneur"; hp = 5; attack = 2; cost = 2; description = "Soigne les alliés pendant le combat."; imageUrl = "https://example.com/images/soigneur.png"; break;
            case 3:
                cardName = "Archer"; hp = 7; attack = 6; cost = 3; description = "Un archer agile et précis."; imageUrl = "https://example.com/images/archer.png"; break;
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
        // Activer le GameObject parent d'abord
        if (AttackOrder != null)
        {
            AttackOrder.SetActive(true);
        }
        
        // Ensuite mettre à jour le texte
        if (attackOrderText != null)
        {
            attackOrderText.text = order.ToString();
            attackOrderText.enabled = true;
            
            // IMPORTANT: Forcer un rebuild de la géométrie du texte
            attackOrderText.ForceMeshUpdate();
            
            // Redimensionner le RectTransform pour accueillir le texte
            var rectTransform = attackOrderText.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Utiliser la préférence de taille du TMP_Text pour dimensionner correctement
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                
                // Ou simplement augmenter la taille pour accommoder le gros font
                rectTransform.sizeDelta = new Vector2(150, -200);

                
                Debug.Log($"[Card] ShowAttackOrder({order}) - RectTransform resizé à {rectTransform.sizeDelta}");
            }
            
            Debug.Log($"[Card] ShowAttackOrder({order}) - text='{attackOrderText.text}', enabled={attackOrderText.enabled}, fontSize={attackOrderText.fontSize}");
        }
        else
        {
            Debug.LogError($"[Card] ShowAttackOrder({order}) - attackOrderText est NULL!");
        }
        
        // Debug pour vérifier
        Debug.Log($"[Card] ShowAttackOrder({order}) - AttackOrder={AttackOrder?.activeSelf}, AttackOrder.activeInHierarchy={AttackOrder?.activeInHierarchy}");
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
        
        Debug.Log($"[Card] ClearAttackOrder - AttackOrder désactivé");
    }


    public void SetSelected(bool selected)
    {
        if (isSelected == selected) return;
        isSelected = selected;

        if (isSelected)
        {
            selectionBaseScale = transform.localScale;
            transform.localScale = selectionBaseScale * selectedScaleMultiplier;
            
            // Vérifier si on est en phase Attack et dans un slot P1
            bool canShowAttackVisuals = false;
            if (PhaseManager.Instance != null && PhaseManager.Instance.CurrentPhase == GamePhase.Attack)
            {
                var slot = GetComponentInParent<CardSlot>();
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
            
            // Désactiver l'outline et l'order d'attaque
            if (AttackOutline != null)
                AttackOutline.SetActive(false);
            
            if (AttackOrder != null)
                AttackOrder.SetActive(false);
        }
    }

    // Utilisé pour marquer les cartes adverses en mode attaque (outline rouge côté P2)
    public void SetOpponentAttacking(bool active)
    {
        if (AttackOutline == null) return;

        AttackOutline.SetActive(active);

        var renderer = AttackOutline.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = active ? Color.red : Color.white;
        }
    }

    // Utilisé pour marquer une carte P1 sélectionnée en défense
    public void SetDefenseSelected(bool active)
    {
        if (DefenseOutline != null)
            DefenseOutline.SetActive(active);
    }

    // Permet de savoir si la carte est marquée comme attaquante (utile côté défense)
    public bool IsAttackingOutlineActive()
    {
        return AttackOutline != null && AttackOutline.activeSelf;
    }
}
