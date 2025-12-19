using UnityEngine;

public class CardSlot : MonoBehaviour
{
    public Card CurrentCard;
    public float targetHeight = 1f; // hauteur souhaitée de la carte dans ce slot (unités monde)

    public bool CanAccept(Card card)
    {
        return CurrentCard == null;
    }

    void OnMouseDown()
    {
        if (HandManager.Instance.SelectedCard != null && CanAccept(HandManager.Instance.SelectedCard))
        {
            Debug.Log("Slot cliqué !");
            HandManager.Instance.PlaceSelectedCardOnSlot(this);
        }
    }

    public void PlaceCard(Card card)
    {
        CurrentCard = card;

        // Attacher au slot sans conserver la transform monde
        card.transform.SetParent(transform, false);

        // Reset transform local de base
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;

        // Calculer la hauteur monde actuelle à partir des Renderers
        var renderers = card.GetComponentsInChildren<Renderer>();
        float worldHeight = 1f;
        if (renderers != null && renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            worldHeight = b.size.y;
        }

        // Normaliser la taille pour obtenir la hauteur voulue
        if (worldHeight > 0f && targetHeight > 0f)
        {
            float scale = targetHeight / worldHeight;
            card.transform.localScale = Vector3.one * scale;
            worldHeight = targetHeight; // après mise à l'échelle
        }

        // Placer la base de la carte au niveau du pivot du slot
        float parentScaleY = transform.lossyScale.y;
        float localY = parentScaleY > 0f ? (worldHeight * 0.5f) / parentScaleY : worldHeight * 0.5f;
        card.transform.localPosition = new Vector3(0f, localY, 0f);

        HandManager.Instance.DeselectCurrentCard();

        // Enregistrer la carte auprès de l'AttackManager si c'est un slot P1
        if (AttackManager.Instance != null && AttackManager.Instance.IsP1BoardSlot(this))
        {
            AttackManager.Instance.RegisterCard(card);
        }

        // Debug mise à l'échelle pour diagnostiquer les cartes allongées
        Debug.Log($"[CardSlot] PlaceCard on {gameObject.name} | targetHeight={targetHeight} | rendererHeight={worldHeight} | parentScaleY={transform.lossyScale.y} | localScale={card.transform.localScale}");
    }
}
