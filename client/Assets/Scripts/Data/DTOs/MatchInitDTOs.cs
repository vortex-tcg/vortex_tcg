using System;
using System.Collections.Generic;

namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class MatchInitUserDto
    {
        public Guid matchId;
        public MatchInitSideDto self = new();
        public MatchInitSideDto opponent = new();
        public int opponentHandSize;
    }

    [Serializable]
    public class MatchInitSideDto
    {
        public int position;
        public MatchInitChampionDto champion = new();
        public int gold;
        public string secondaryCurrencyName = "";
        public int secondaryCurrency;
        public List<MatchInitCardDto> drawnCards = new();
    }

    [Serializable]
    public class MatchInitChampionDto
    {
        public string name = "";
        public string description = "";
    }

    [Serializable]
    public class MatchInitCardDto
    {
        public int gameCardId;
        public string name = "";
        public int hp;
        public int attack;
        public int cost;
        public string description = "";
        public int cardType;
        public List<string> classes = new();
        public List<string> states = new();
        public string imageUrl = "";
    }
}
