using UnityEngine;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.UI;
using VortexTCG.Scripts.Features.Match.Services;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardSlotUI : MonoBehaviour
    {
        [Header("Index")]
        public int slotIndex = 0;

        [Header("Slot options")]
        public bool isOpponentSlot = false;
        public CardUI CurrentCard;

        public bool CanAccept(CardUI card) => CurrentCard == null;

        private void OnMouseDown()
        {
            if (isOpponentSlot)
            {
                Debug.Log($"[CardSlot] CLICK ignored - opponent slot");
                return;
            }

            Debug.Log($"[CardSlot] ✅ CLICK slotIndex={slotIndex}");

            // Déclencher événement pour demander le jeu de carte
            // Les services gèreront la validation et l'appel serveur
            CardUI selectedCard = HandUI.Instance?.SelectedCard;
            
            Debug.Log($"[CardSlot] HandUI.Instance={(HandUI.Instance != null ? "EXISTS" : "NULL")}");
            Debug.Log($"[CardSlot] SelectedCard={(selectedCard != null ? selectedCard.cardName : "NULL")}");
            Debug.Log($"[CardSlot] CurrentCard={(CurrentCard != null ? CurrentCard.cardName : "NULL")}");
            Debug.Log($"[CardSlot] CanAccept={CanAccept(selectedCard)}");
            
            if (selectedCard != null && CanAccept(selectedCard))
            {
                Debug.Log($"[CardSlot] ✅✅✅ Firing CardPlayRequested for card '{selectedCard.cardName}' to slot {slotIndex}");
                MatchEvents.FireCardPlayRequested(selectedCard, this);
            }
            else
            {
                if (selectedCard == null)
                    Debug.LogWarning($"[CardSlot] ❌ Cannot play - No card selected in hand");
                else
                    Debug.LogWarning($"[CardSlot] ❌ Cannot play - Slot already occupied or cannot accept");
            }
        }

        /// <summary>
        /// Pose une carte sur ce slot du board
        /// Condition: la carte doit être sélectionnée et les conditions doivent être remplies
        /// Déplace physiquement la carte du slot de la main vers ce slot du board
        /// </summary>
        public void PlaceCard(CardUI card)
        {
            if (card == null) return;

            Debug.Log($"[CardSlotUI.PlaceCard] ✅ Posing card '{card.cardName}' on slot {slotIndex}");

            // Vérifier que le slot est vide
            if (CurrentCard != null)
            {
                Debug.LogWarning($"[CardSlotUI.PlaceCard] ❌ Slot {slotIndex} already occupied!");
                return;
            }

            // Marquer la carte comme placée
            CurrentCard = card;
            
            // Déplacer la carte du slot de la main vers ce slot du board
            Transform cardTransform = card.transform;
            cardTransform.SetParent(transform, false);  // Changement du parent (du slot main au slot board)
            cardTransform.localPosition = Vector3.zero;
            cardTransform.localRotation = Quaternion.identity;
            cardTransform.localScale = Vector3.one;

            Debug.Log($"[CardSlotUI.PlaceCard] ✅ Card '{card.cardName}' moved to slot {slotIndex}");

            // Enregistrer la carte auprès du service d'attaque si c'est un slot joueur
            if (!isOpponentSlot)
            {
                AttackService attackService = AttackUI.Instance?.AttackService;
                if (attackService != null)
                {
                    attackService.RegisterCard(card);
                    Debug.Log($"[CardSlotUI.PlaceCard] ✅ Card registered with AttackService");
                }
            }
        }

        /// <summary>
        /// Remplace une carte déjà placée par une autre
        /// </summary>
        public void ReplaceCard(CardUI newCard)
        {
            if (newCard == null) return;

            // Supprimer la carte actuelle si elle existe
            if (CurrentCard != null)
            {
                Destroy(CurrentCard.gameObject);
            }

            // Placer la nouvelle carte
            PlaceCard(newCard);
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
