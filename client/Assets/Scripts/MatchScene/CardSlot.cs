using UnityEngine;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardSlot : MonoBehaviour
    {
        [Header("Index board 0..4")]
        public int slotIndex = 0;

        [Header("Slot options")]
        public bool isOpponentSlot = false;
        public Card CurrentCard;
        public float targetHeight = 1.0f;

        public bool CanAccept(Card card) => CurrentCard == null;

        private void OnMouseDown()
        {
            if (isOpponentSlot) return;

            Debug.Log(
                $"[CardSlot] CLICK slotIndex={slotIndex} " +
                $"phase={PhaseManager.Instance?.CurrentPhase} " +
                $"selected={(HandManager.Instance?.SelectedCard != null)} " +
                $"canAccept={(HandManager.Instance?.SelectedCard != null ? CanAccept(HandManager.Instance.SelectedCard) : false)}"
            );

            if (HandManager.Instance == null) return;
            if (HandManager.Instance.SelectedCard == null) return;
            if (!CanAccept(HandManager.Instance.SelectedCard)) return;

            _ = HandManager.Instance.RequestPlaySelectedCard(this);
        }

        public void PlaceCard(Card card)
        {
            if (card == null) return;

            CurrentCard = card;
            Transform t = card.transform;
            t.SetParent(transform, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            t.localPosition = new Vector3(0f, 1f, 0f);

            if (AttackManager.Instance != null && AttackManager.Instance.IsP1BoardSlot(this))
                AttackManager.Instance.RegisterCard(card);

            if (DefenseManager.Instance != null)
                DefenseManager.Instance.RegisterCard(card);

            Debug.Log($"[CardSlot] PlaceCard slot='{name}' isOpp={isOpponentSlot} " +
                      $"slotLossy={transform.lossyScale} cardLocalScale={t.localScale}");
        }
    }
}
