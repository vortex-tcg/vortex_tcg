using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.UI
{
    /// <summary>
    /// Gestionnaire UI du cimetière du joueur
    /// Remplace GraveyardManager en utilisant MatchEvents
    /// </summary>
    public class GraveyardUI : MonoBehaviour
    {
        public static GraveyardUI Instance { get; private set; }

        [SerializeField] private Transform _graveyardRoot;
        [SerializeField] private CardUI _graveCardPrefab;

        private readonly List<CardUI> _graveCards = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            MatchEvents.OnGameStarted += HandleGameStarted;
            MatchEvents.OnPlayerCardsDrawn += HandleCardsDrawn;
        }

        private void OnDisable()
        {
            MatchEvents.OnGameStarted -= HandleGameStarted;
            MatchEvents.OnPlayerCardsDrawn -= HandleCardsDrawn;
        }

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            ResetGraveyard();
        }

        private void HandleCardsDrawn(DrawResultForPlayerDto result)
        {
            if (result?.SentToGraveyard != null && result.SentToGraveyard.Count > 0)
                AddCards(result.SentToGraveyard);
        }

        public void ResetGraveyard()
        {
            foreach (CardUI card in _graveCards)
            {
                Destroy(card.gameObject);
            }
            _graveCards.Clear();
        }

        public void AddCards(List<DrawnCardDto> cards)
        {
            if (cards == null || cards.Count == 0) return;

            if (_graveCardPrefab == null || _graveyardRoot == null)
            {
                Debug.LogError("[GraveyardUI] Prefab ou Root non assigné.");
                return;
            }

            foreach (DrawnCardDto dto in cards)
            {
                CardUI card = Instantiate(_graveCardPrefab, _graveyardRoot);
                card.ApplyDTO(
                    dto.GameCardId.ToString(),
                    dto.Name,
                    dto.Hp,
                    dto.Attack,
                    dto.Cost,
                    dto.Description,
                    ""
                );
                _graveCards.Add(card);
            }

            Debug.Log($"[GraveyardUI] Added {cards.Count} cards to graveyard. Total: {_graveCards.Count}");
        }
    }
}
