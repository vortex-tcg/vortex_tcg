using System.Collections.Generic;
using UnityEngine;
using DrawDTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class GraveyardManager : MonoBehaviour
    {
        private List<DrawnCardDto> _cards = new();

        public int Count => _cards?.Count ?? 0;
        public IReadOnlyList<DrawnCardDto> Cards => _cards;

        public void SetGraveyard(List<DrawnCardDto> cards)
        {
            _cards = cards ?? new List<DrawnCardDto>();
            Debug.Log($"[GraveyardManager] SetGraveyard count={_cards.Count}");
        }

        public void AddCards(List<DrawnCardDto> cards)
        {
            if (cards == null || cards.Count == 0) return;
            _cards ??= new List<DrawnCardDto>();
            _cards.AddRange(cards);
            Debug.Log($"[GraveyardManager] AddCards +{cards.Count} total={_cards.Count}");
        }

        public void ResetGraveyard()
        {
            _cards?.Clear();
            Debug.Log("[GraveyardManager] Reset");
        }
    }
}