using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [HideInInspector]
    public Card SelectedCard;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SelectCard(Card card)
    {
        // V�rifier si on peut s�lectionner la carte selon la phase
        if (PhaseManager.Instance.CurrentPhase == GamePhase.Attack)
        {
            Debug.Log("Impossible de s�lectionner pendant la phase d'attaque !");
            return;
        }

        if (SelectedCard != null)
            SelectedCard.SetSelected(false);

        SelectedCard = card;
        SelectedCard.SetSelected(true);
    }

    public void DeselectCurrentCard()
    {
        if (SelectedCard != null)
        {
            SelectedCard.SetSelected(false);
            SelectedCard = null;
        }
    }

    public void PlaceSelectedCardOnSlot(CardSlot slot)
    {
        if (SelectedCard == null) return;

        // V�rifier la phase avant de placer
        if (PhaseManager.Instance.CurrentPhase != GamePhase.StandBy)
        {
            Debug.Log("Impossible de placer la carte dans cette phase !");
            return;
        }

        if (!slot.CanAccept(SelectedCard)) return;

        slot.PlaceCard(SelectedCard);
        SelectedCard = null;
    }
}
