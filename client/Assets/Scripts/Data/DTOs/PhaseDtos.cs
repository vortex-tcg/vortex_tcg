using System;

namespace VortexTCG.Scripts.DTOs
{
    public enum GamePhase
    {
        PLACEMENT = 0,
        ATTACK = 1,
        DEFENSE = 2,
        END_TURN = 3
    }

    [Serializable]
    public class PhaseChangeResultDTO
    {
        public GamePhase CurrentPhase { get; set; }
        public Guid ActivePlayerId { get; set; }
        public int TurnNumber { get; set; }
        public bool AutoChanged { get; set; }
        public string AutoChangeReason { get; set; }
        public bool CanAct { get; set; }
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