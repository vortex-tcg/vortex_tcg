using System.Collections.Generic;
using UnityEngine;
using DrawDTOs;

public class GraveyardManager : MonoBehaviour
{
    private List<DrawnCardDTO> _cards = new();

    public int Count => _cards?.Count ?? 0;
    public IReadOnlyList<DrawnCardDTO> Cards => _cards;

    public void SetGraveyard(List<DrawnCardDTO> cards)
    {
        _cards = cards ?? new List<DrawnCardDTO>();
        Debug.Log($"[GraveyardManager] SetGraveyard count={_cards.Count}");
    }

    public void AddCards(List<DrawnCardDTO> cards)
    {
        if (cards == null || cards.Count == 0) return;
        _cards ??= new List<DrawnCardDTO>();
        _cards.AddRange(cards);
        Debug.Log($"[GraveyardManager] AddCards +{cards.Count} total={_cards.Count}");
    }

    public void ResetGraveyard()
    {
        _cards?.Clear();
        Debug.Log("[GraveyardManager] Reset");
    }
}