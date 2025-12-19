using UnityEngine;
using TMPro;

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
    [SerializeField] private float selectedScaleMultiplier = 1.08f;
    private bool isSelected;
    private Vector3 selectionBaseScale;

    void Awake()
    {
        selectionBaseScale = transform.localScale;
        MockData();
        RefreshUI();
        
        // Masquer le texte d'ordre d'attaque au démarrage
        if (attackOrderText != null)
            attackOrderText.gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        Debug.Log("Carte cliquée !");

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
        if (attackOrderText != null)
        {
            attackOrderText.text = order.ToString();
            attackOrderText.gameObject.SetActive(true);
        }
    }

    public void ClearAttackOrder()
    {
        if (attackOrderText != null)
        {
            attackOrderText.text = "";
            attackOrderText.gameObject.SetActive(false);
        }
    }


    public void SetSelected(bool selected)
    {
        if (isSelected == selected) return;
        isSelected = selected;

        if (isSelected)
        {
            selectionBaseScale = transform.localScale;
            transform.localScale = selectionBaseScale * selectedScaleMultiplier;
        }
        else
        {
            transform.localScale = selectionBaseScale;
        }
    }
}
