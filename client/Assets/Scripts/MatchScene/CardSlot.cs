using UnityEngine;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardSlot : MonoBehaviour
    {
        [Header("Index board 0..4")]
        public int slotIndex = 0;

        public Card CurrentCard;
        public float targetHeight = 1.0f;

        public bool CanAccept(Card card) => CurrentCard == null;

        private void OnMouseDown()
        {
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

            Vector3 baseScale = card.transform.localScale;
            card.transform.SetParent(transform, false);
            card.transform.localRotation = Quaternion.identity;

            Renderer[] renderers = card.GetComponentsInChildren<Renderer>();
            float worldHeight = 1f;

            if (renderers != null && renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    b.Encapsulate(renderers[i].bounds);

                worldHeight = b.size.y;
            }

            if (worldHeight > 0f && targetHeight > 0f)
            {
                float scale = targetHeight / worldHeight;
                card.transform.localScale = baseScale * scale;
                worldHeight = targetHeight;
            }

            float parentScaleY = transform.lossyScale.y;
            float localY = (parentScaleY > 0f ? (worldHeight * 0.5f) / parentScaleY : worldHeight * 0.5f);
            card.transform.localPosition = new Vector3(0f, localY, 0f);

            if (AttackManager.Instance != null && AttackManager.Instance.IsP1BoardSlot(this))
                AttackManager.Instance.RegisterCard(card);
        }
    }
}
