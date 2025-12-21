using UnityEngine;

namespace VortexTCG.Scripts.MatchScene
{
    public class CardSlot : MonoBehaviour
    {
        public Card CurrentCard;
        public float targetHeight = 1.0f; // hauteur souhaitée de la carte dans ce slot (unités monde)

        public bool CanAccept(Card card)
        {
            return CurrentCard == null;
        }

        void OnMouseDown()
        {
            if (HandManager.Instance.SelectedCard != null && CanAccept(HandManager.Instance.SelectedCard))
            {
                HandManager.Instance.PlaceSelectedCardOnSlot(this);
            }
        }

        public void PlaceCard(Card card)
        {
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
            card.transform.localScale = Vector3.one;

            // HandManager.Instance.DeselectCurrentCard();

            if (AttackManager.Instance != null && AttackManager.Instance.IsP1BoardSlot(this))
            {
                AttackManager.Instance.RegisterCard(card);
            }
        }
    }
}