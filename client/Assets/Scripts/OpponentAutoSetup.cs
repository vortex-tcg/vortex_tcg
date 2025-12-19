using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Auto-spawns an opponent (P2) hand and board with random cards for testing defense logic.
/// Attach this to a GameObject in the scene and wire references in the inspector.
/// </summary>
public class OpponentAutoSetup : MonoBehaviour
{
    [Header("Prefabs & Parents")]
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Transform p2HandParent;

    [Header("P2 Board Slots")] 
    [SerializeField] private List<CardSlot> p2BoardSlots = new List<CardSlot>();

    [Header("Generation Settings")]
    [SerializeField] private int handSize = 5;
    [SerializeField] private int minBoardCards = 2;
    [SerializeField] private int maxBoardCards = 4;
    [SerializeField] private float boardTargetHeight = 1.0f;

    [Header("Run Control")]
    [SerializeField] private bool runOnStart = true;

    private void Start()
    {
        if (runOnStart)
        {
            GenerateOpponentState();
        }
    }

    [ContextMenu("Generate Opponent State")]
    public void GenerateOpponentState()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("OpponentAutoSetup: cardPrefab is not assigned");
            return;
        }

        if (p2HandParent != null)
        {
            for (int i = p2HandParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(p2HandParent.GetChild(i).gameObject);
            }
        }

        foreach (CardSlot slot in p2BoardSlots)
        {
            if (slot == null) continue;
            if (slot.CurrentCard != null)
            {
                DestroyImmediate(slot.CurrentCard.gameObject);
                slot.CurrentCard = null;
            }
        }

        for (int i = 0; i < handSize; i++)
        {
            Card handCard = Instantiate(cardPrefab, p2HandParent ? p2HandParent : transform);
            ApplyRandomData(handCard, i);
        }

        int boardCards = Mathf.Clamp(Random.Range(minBoardCards, maxBoardCards + 1), 0, p2BoardSlots.Count);
        List<int> slotIndices = new List<int>();
        for (int i = 0; i < p2BoardSlots.Count; i++) slotIndices.Add(i);
        Shuffle(slotIndices);

        for (int i = 0; i < boardCards; i++)
        {
            CardSlot slot = p2BoardSlots[slotIndices[i]];
            if (slot == null) continue;

            Card card = Instantiate(cardPrefab);
            ApplyRandomData(card, i + 1000);

            slot.PlaceCard(card);

            bool setAttacking = Random.value > 0.5f;
            card.SetOpponentAttacking(setAttacking);

            if (boardTargetHeight > 0f)
            {
                Renderer[] renderers = card.GetComponentsInChildren<Renderer>();
                float worldHeight = 1f;
                if (renderers != null && renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    for (int r = 1; r < renderers.Length; r++) b.Encapsulate(renderers[r].bounds);
                    worldHeight = b.size.y;
                }
                if (worldHeight > 0f)
                {
                    float scale = boardTargetHeight / worldHeight;
                    card.transform.localScale = Vector3.one * scale;
                }
            }
        }
    }

    private void ApplyRandomData(Card card, int seedOffset)
    {
        if (card == null) return;
        Random.InitState(System.Environment.TickCount + seedOffset);
        int roll = Random.Range(0, 4);
        switch (roll)
        {
            case 0: card.ApplyDTO(System.Guid.NewGuid().ToString(), "P2 Guerrier", 10, 5, 3, "Guerrier adverse", ""); break;
            case 1: card.ApplyDTO(System.Guid.NewGuid().ToString(), "P2 Mage", 6, 8, 4, "Mage adverse", ""); break;
            case 2: card.ApplyDTO(System.Guid.NewGuid().ToString(), "P2 Soigneur", 5, 2, 2, "Soigneur adverse", ""); break;
            default: card.ApplyDTO(System.Guid.NewGuid().ToString(), "P2 Archer", 7, 6, 3, "Archer adverse", ""); break;
        }
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
