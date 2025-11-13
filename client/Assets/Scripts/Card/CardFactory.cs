using UnityEngine;

public static class CardFactory
{

    public static Card3D SpawnFromValues(
        GameObject card3dPrefab,
        Transform parent,
        Vector3 position,
        Quaternion rotation,
        string id,
        string displayName,
        string description,
        CardKind kind,
        int cost,
        int baseAttack = 0,
        int baseHealth = 0)
    {
        var go = Object.Instantiate(card3dPrefab, position, rotation, parent);
        var card = go.GetComponent<Card3D>();
        if (!card) card = go.GetComponentInChildren<Card3D>();

        var payload = new CardPayload {
            id = id,
            displayName = displayName,
            description = description,
            kind = kind,
            cost = cost,
            baseAttack = baseAttack,
            baseHealth = baseHealth
        };

        card.Setup(payload); 
        return card;
    }

    public static Card3D SpawnFromPayload(
        GameObject card3dPrefab,
        Transform parent,
        Vector3 position,
        Quaternion rotation,
        CardPayload payload)
    {
        var go = Object.Instantiate(card3dPrefab, position, rotation, parent);
        var card = go.GetComponent<Card3D>();
        if (!card) card = go.GetComponentInChildren<Card3D>();
        card.Setup(payload);
        return card;
    }
}
