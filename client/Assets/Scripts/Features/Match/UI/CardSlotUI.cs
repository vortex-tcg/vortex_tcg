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

        private Collider slotCollider;

        public bool CanAccept(CardUI card) => CurrentCard == null;

        private void Awake()
        {
            AutoAssignBoardSlotIndexIfNeeded();
            slotCollider = GetComponent<Collider>();
            UpdateSlotColliderState();
        }

        private void AutoAssignBoardSlotIndexIfNeeded()
        {
            // Some scenes keep slotIndex at default 0; infer board index from slot name to keep server positions stable.
            if (slotIndex != 0)
                return;

            string n = name ?? string.Empty;
            bool isBoardSlot = n.Contains("BoardSlot");
            if (!isBoardSlot)
                return;

            int parsed = ParseTrailingNumber(n);
            if (parsed <= 0)
                return;

            slotIndex = parsed - 1;
            Debug.Log($"[CardSlotUI] Auto-assigned board slotIndex={slotIndex} from name='{name}'");
        }

        private static int ParseTrailingNumber(string value)
        {
            if (string.IsNullOrEmpty(value))
                return -1;

            int i = value.Length - 1;
            while (i >= 0 && char.IsDigit(value[i]))
                i--;

            int start = i + 1;
            if (start >= value.Length)
                return -1;

            string numeric = value.Substring(start);
            return int.TryParse(numeric, out int parsed) ? parsed : -1;
        }

        private void OnMouseDown()
        {
            if (isOpponentSlot)
            {
                Debug.Log($"[CardSlot] CLICK ignored - opponent slot");
                return;
            }

            Debug.Log($"[CardSlot] ✅ CLICK slotIndex={slotIndex}");
            CardUI selectedCard = HandUI.Instance?.SelectedCard;
            
            Debug.Log($"[CardSlot] HandUI.Instance={(HandUI.Instance != null ? "EXISTS" : "NULL")}");
            Debug.Log($"[CardSlot] SelectedCard={(selectedCard != null ? selectedCard.cardName : "NULL")}");
            Debug.Log($"[CardSlot] CurrentCard={(CurrentCard != null ? CurrentCard.cardName : "NULL")}");
            Debug.Log($"[CardSlot] CanAccept={CanAccept(selectedCard)}");

            if (CurrentCard != null && selectedCard == null)
            {
                Debug.Log($"[CardSlot] Forwarding click to board card '{CurrentCard.cardName}'");
                MatchEvents.FireCardClicked(CurrentCard);
                return;
            }
            
            if (selectedCard != null && CanAccept(selectedCard))
            {
                Debug.Log($"[CardSlot] ✅✅✅ Firing CardPlayRequested for card '{selectedCard.cardName}' to slot {slotIndex}");
                MatchEvents.FireCardPlayRequested(selectedCard, this);
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

            CardSlotUI previousSlot = card.GetComponentInParent<CardSlotUI>();
            if (previousSlot != null && previousSlot != this && previousSlot.CurrentCard == card)
            {
                previousSlot.SetCurrentCard(null);
            }

            Debug.Log($"[CardSlotUI.PlaceCard] ✅ Posing card '{card.cardName}' on slot {slotIndex}");
            if (CurrentCard != null)
            {
                Debug.LogWarning($"[CardSlotUI.PlaceCard] ❌ Slot {slotIndex} already occupied!");
                return;
            }

            SetCurrentCard(card);
            
            Transform cardTransform = card.transform;
            cardTransform.SetParent(transform, false);  // Changement du parent (du slot main au slot board)
            cardTransform.localPosition = Vector3.zero;
            cardTransform.localRotation = Quaternion.identity;
            cardTransform.localScale = Vector3.one;
            card.RefreshCurrentHpVisibility();

            Debug.Log($"[CardSlotUI.PlaceCard] ✅ Card '{card.cardName}' moved to slot {slotIndex}");
            if (!isOpponentSlot)
            {
                AttackService attackService = AttackUI.Instance?.AttackService;
                if (attackService != null)
                {
                    attackService.RegisterCard(card);
                    Debug.Log($"[CardSlotUI.PlaceCard] ✅ Card registered with AttackService");
                }

                // also inform defense UI so newly placed cards are tracked immediately
                DefenseUI.Instance?.RegisterCard(card);

                // during the first turn, newly placed cards are sleepy
                if (PhaseService.Instance != null && PhaseService.Instance.CurrentTurn == 1)
                {
                    card.SetSleepy(true);
                }
            }

            UpdateSlotColliderState();
        }

        public void ReplaceCard(CardUI newCard)
        {
            if (newCard == null) return;
            if (CurrentCard != null)
            {
                Destroy(CurrentCard.gameObject);
                SetCurrentCard(null);
            }

            PlaceCard(newCard);
        }

        public void ClearSlot()
        {
            if (CurrentCard != null)
            {
                Destroy(CurrentCard.gameObject);
                SetCurrentCard(null);
            }
        }

        public void SetCurrentCard(CardUI card)
        {
            CurrentCard = card;
            UpdateSlotColliderState();
        }

        public void RefreshSlotColliderState()
        {
            UpdateSlotColliderState();
        }

        private void UpdateSlotColliderState()
        {
            if (slotCollider == null)
                return;

            bool hasCard = CurrentCard != null;
            slotCollider.enabled = !hasCard;
        }
    }
}
