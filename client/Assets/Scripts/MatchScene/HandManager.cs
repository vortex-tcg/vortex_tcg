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
        if (PhaseManager.Instance.CurrentPhase == GamePhase.Attack)
        {
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

        if (PhaseManager.Instance.CurrentPhase != GamePhase.StandBy)
        {
            return;
        }

        if (!slot.CanAccept(SelectedCard)) return;

        slot.PlaceCard(SelectedCard);
        SelectedCard = null;
    }
}
