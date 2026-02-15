using UnityEngine;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.UI;
using VortexTCG.Scripts.Features.Match.Services;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardSlotUI : MonoBehaviour
    {
        [Header("Index board 0..4")]
        public int slotIndex = 0;

        [Header("Slot options")]
        public bool isOpponentSlot = false;
        public CardUI CurrentCard;
        public float targetHeight = 1.0f;

        public bool CanAccept(CardUI card) => CurrentCard == null;

        private void OnMouseDown()
        {
            if (isOpponentSlot) return;

            Debug.Log($"[CardSlot] CLICK slotIndex={slotIndex}");

            // Déclencher événement pour demander le jeu de carte
            // Les services gèreront la validation et l'appel serveur
            CardUI selectedCard = HandUI.Instance?.SelectedCard;
            if (selectedCard != null && CanAccept(selectedCard))
            {
                MatchEvents.FireCardPlayRequested(selectedCard, this);
            }
        }

        public void PlaceCard(CardUI card)
        {
            if (card == null) return;

            CurrentCard = card;
            Transform t = card.transform;
            t.SetParent(transform, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            t.localPosition = new Vector3(0f, 1f, 0f);

            // Ajuster la position Y du collider à 200
            Collider col = card.GetComponent<Collider>();
            if (col != null)
            {
                if (col is BoxCollider boxCol)
                {
                    boxCol.center = new Vector3(boxCol.center.x, 200f, boxCol.center.z);
                    Debug.Log($"[CardSlot] Collider center ajusté à Y=200 pour {card.cardName}");
                }
            }

            // Enregistrer la carte auprès du service d'attaque si c'est un slot joueur
            AttackService attackService = AttackService.Instance;
            if (attackService != null && !isOpponentSlot)
            {
                attackService.RegisterCard(card);
            }
        }

        public void ClearSlot()
        {
            if (CurrentCard != null)
            {
                Destroy(CurrentCard.gameObject);
                CurrentCard = null;
            }
        }
    }
}
