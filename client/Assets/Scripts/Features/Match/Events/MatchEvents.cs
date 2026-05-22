using System;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Events
{
    /// <summary>
    /// Types d'événements du Match - utilisés pour découpler les services et UI
    /// </summary>
    public static class MatchEvents
    {
        
        /// <summary>Jeu démarré, initialisation complète</summary>
        public static event Action<PhaseChangeResultDTO> OnGameStarted;
        
        /// <summary>Phase changée (Main, Placement, Attack, Defense, StandBy)</summary>
        public static event Action<PhaseChangeResultDTO> OnPhaseChanged;
        
        /// <summary>Demande de changement de phase</summary>
        public static event Action OnPhaseChangeRequested;

        
        /// <summary>Cartes piochées pour le joueur</summary>
        public static event Action<DrawResultForPlayerDto> OnPlayerCardsDrawn;
        
        /// <summary>Cartes piochées pour l'adversaire (visibilité limitée)</summary>
        public static event Action<DrawResultForOpponentDto> OnOpponentCardsDrawn;

        
        /// <summary>Résultat du jeu d'une carte du joueur</summary>
        public static event Action<PlayCardPlayerResultDto> OnPlayerCardPlayed;
        
        /// <summary>Résultat du jeu d'une carte adversaire</summary>
        public static event Action<PlayCardOpponentResultDto> OnOpponentCardPlayed;
        
        /// <summary>Demande d'annulation du jeu pendant en attente</summary>
        public static event Action<string> OnPendingPlayCancelled;

        
        /// <summary>Engagement en attaque</summary>
        public static event Action<AttackResponseDto> OnPlayerAttackEngaged;
        
        /// <summary>Engagement adversaire en attaque</summary>
        public static event Action<AttackResponseDto> OnOpponentAttackEngaged;
        
        /// <summary>Engagement en défense</summary>
        public static event Action<DefenseDataResponseDto> OnPlayerDefenseEngaged;
        
        /// <summary>Engagement adversaire en défense</summary>
        public static event Action<DefenseDataResponseDto> OnOpponentDefenseEngaged;
        
        /// <summary>Résolution d'une bataille</summary>
        public static event Action<BattlesDataDto, bool> OnBattleResolution;

        
        /// <summary>Carte sélectionnée en main</summary>
        public static event Action<CardUI> OnCardSelected;
        
        /// <summary>Carte désélectionnée en main</summary>
        public static event Action OnCardDeselected;
        
        /// <summary>Tentative de jeu d'une carte</summary>
        public static event Action<CardUI, CardSlotUI> OnCardPlayRequested;
        
        /// <summary>Carte cliquée (peut être main ou plateau)</summary>
        public static event Action<CardUI> OnCardClicked;

        
        /// <summary>Message de statut du serveur</summary>
        public static event Action<string> OnServerStatusMessage;

        public static void FireGameStarted(PhaseChangeResultDTO result) => OnGameStarted?.Invoke(result);
        public static void FirePhaseChanged(PhaseChangeResultDTO result) => OnPhaseChanged?.Invoke(result);
        public static void FirePhaseChangeRequested() => OnPhaseChangeRequested?.Invoke();

        public static void FirePlayerCardsDrawn(DrawResultForPlayerDto result) => OnPlayerCardsDrawn?.Invoke(result);
        public static void FireOpponentCardsDrawn(DrawResultForOpponentDto result) => OnOpponentCardsDrawn?.Invoke(result);

        public static void FirePlayerCardPlayed(PlayCardPlayerResultDto result)
        {
            Debug.Log($"[MatchEvents] 📢 FirePlayerCardPlayed invoked - subscribers: {OnPlayerCardPlayed?.GetInvocationList().Length ?? 0}");
            OnPlayerCardPlayed?.Invoke(result);
        }

        public static void FireOpponentCardPlayed(PlayCardOpponentResultDto result)
        {
            Debug.Log($"[MatchEvents] 📢 FireOpponentCardPlayed invoked - subscribers: {OnOpponentCardPlayed?.GetInvocationList().Length ?? 0}");
            OnOpponentCardPlayed?.Invoke(result);
        }

        public static void FirePendingPlayCancelled(string reason) => OnPendingPlayCancelled?.Invoke(reason);

        public static void FirePlayerAttackEngaged(AttackResponseDto dto) => OnPlayerAttackEngaged?.Invoke(dto);
        public static void FireOpponentAttackEngaged(AttackResponseDto dto) => OnOpponentAttackEngaged?.Invoke(dto);
        public static void FirePlayerDefenseEngaged(DefenseDataResponseDto dto) => OnPlayerDefenseEngaged?.Invoke(dto);
        public static void FireOpponentDefenseEngaged(DefenseDataResponseDto dto) => OnOpponentDefenseEngaged?.Invoke(dto);
        public static void FireBattleResolution(BattlesDataDto dto, bool localIsAttacker) => OnBattleResolution?.Invoke(dto, localIsAttacker);

        public static void FireCardSelected(CardUI card) => OnCardSelected?.Invoke(card);
        public static void FireCardDeselected() => OnCardDeselected?.Invoke();
        public static void FireCardPlayRequested(CardUI card, CardSlotUI slot) => OnCardPlayRequested?.Invoke(card, slot);
        public static void FireCardClicked(CardUI card) => OnCardClicked?.Invoke(card);

        public static void FireServerStatusMessage(string message) => OnServerStatusMessage?.Invoke(message);

        public static void ResetAll()
        {
            OnGameStarted = null;
            OnPhaseChanged = null;
            OnPhaseChangeRequested = null;
            OnPlayerCardsDrawn = null;
            OnOpponentCardsDrawn = null;
            OnPlayerCardPlayed = null;
            OnOpponentCardPlayed = null;
            OnPendingPlayCancelled = null;
            OnPlayerAttackEngaged = null;
            OnOpponentAttackEngaged = null;
            OnPlayerDefenseEngaged = null;
            OnOpponentDefenseEngaged = null;
            OnBattleResolution = null;
            OnCardSelected = null;
            OnCardDeselected = null;
            OnCardPlayRequested = null;
            OnCardClicked = null;
            OnServerStatusMessage = null;
        }
    }
}
