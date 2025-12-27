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
            Vector3 baseScale = t.localScale; 
            t.SetParent(transform, false);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale    = baseScale;
            Renderer[] renderers = card.GetComponentsInChildren<Renderer>(true);
            float refSize = 1f;

            if (renderers != null && renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    b.Encapsulate(renderers[i].bounds);

                float sizeY  = b.size.y;
                float sizeXZ = Mathf.Max(b.size.x, b.size.z);
                refSize = (sizeY > 0.0005f) ? sizeY : sizeXZ;
                if (refSize <= 0.0005f) refSize = 1f;
            }
            float scaleFactor = (targetHeight > 0f) ? (targetHeight / refSize) : 1f;
            scaleFactor = Mathf.Clamp(scaleFactor, 0.01f, 10f);
            t.localScale = baseScale * scaleFactor;
            float parentScaleY = transform.lossyScale.y;
            if (Mathf.Abs(parentScaleY) < 0.0001f) parentScaleY = 1f;

            float localY = (targetHeight * 0.5f) / parentScaleY;
            t.localPosition = new Vector3(0f, localY, 0f);
            if (AttackManager.Instance != null && AttackManager.Instance.IsP1BoardSlot(this))
                AttackManager.Instance.RegisterCard(card);
            Debug.Log($"[CardSlot] PlaceCard slot='{name}' isOpp={isOpponentSlot} " +
                      $"slotLossy={transform.lossyScale} cardLocalScale={t.localScale} scaleFactor={scaleFactor}");
        }

      
    }
}
