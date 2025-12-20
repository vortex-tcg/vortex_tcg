using System;
using System.Collections.Generic;
using VortexTCG.Scripts.DTOs;
namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class DrawnCardDto
    {
        public string Id { get; set; }           
        public int GameCardId { get; set; }
        public string Name { get; set; }
        public int Hp { get; set; }
        public int Attack { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; }
        public CardType CardType { get; set; }
    }

    [Serializable]
    public class DrawResultForPlayerDto
    {
        public List<DrawnCardDto> DrawnCards { get; set; } = new();
        public int FatigueCount { get; set; }
        public int BaseFatigue { get; set; }
        public List<DrawnCardDto> SentToGraveyard{ get; set; } = new(); 
    }

    [Serializable]
    public class DrawResultForOpponentDto
    {
        public string PlayerId { get; set; }
        public int CardsDrawnCount { get; set; }
        public int CardsBurnedCount { get; set; }
        public int FatigueCount { get; set; }
        public int BaseFatigue { get; set; }
    }
}