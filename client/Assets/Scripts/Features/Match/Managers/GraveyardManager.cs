using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class GraveyardManager : MonoBehaviour
    {
        public static GraveyardManager Instance { get; private set; }

        private readonly List<DrawnCardDto> _cards = new();

        public int Count => _cards.Count;
        public IReadOnlyList<DrawnCardDto> Cards => _cards;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void ResetGraveyard() => _cards.Clear();

        public void AddCards(List<DrawnCardDto> cards)
        {
            if (cards == null || cards.Count == 0) return;
            _cards.AddRange(cards);
            Debug.Log($"[GraveyardManager] +{cards.Count} (total={_cards.Count})");
        }
    }
}