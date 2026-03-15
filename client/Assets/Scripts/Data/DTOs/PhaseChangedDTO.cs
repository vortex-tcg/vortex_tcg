using System;

namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class PhaseChangedDto
    {
        public Guid matchId;
        public int currentPlayerPosition;
        public int phase;
    }
}
