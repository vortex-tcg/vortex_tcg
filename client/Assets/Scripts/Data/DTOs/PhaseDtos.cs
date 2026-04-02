using System;

namespace VortexTCG.Scripts.DTOs
{
    public enum GamePhase
    {
        PLACEMENT = 1,
        ATTACK = 2,
        DEFENSE = 3,
        END_TURN = 4
    }

    [Serializable]
    public class PhaseChangeResultDTO
    {
        public GamePhase CurrentPhase { get; set; }
        public Guid ActivePlayerId { get; set; }
        public int TurnNumber { get; set; }
        public bool AutoChanged { get; set; }
        public string? AutoChangeReason { get; set; }
        public bool CanAct { get; set; }
        public long? TimerEndTime { get; set; } // Unix timestamp (ms) de fin du timer
    }

    [Serializable]
    public class PhaseChangeForOpponentDTO
    {
        public GamePhase CurrentPhase { get; set; }
        public Guid ActivePlayerId { get; set; }
        public int TurnNumber { get; set; }
        public bool IsYourTurnToAct { get; set; }
    }

    [Serializable]
    public class ChangePhaseResultDTO
    {
        public PhaseChangeResultDTO ActivePlayerResult { get; set; } = new();
        public PhaseChangeForOpponentDTO OpponentResult { get; set; } = new();
        public bool TurnChanged { get; set; }
    }
}