using System;
using System.Collections.Generic;

namespace DrawDTOs
{
    [Serializable]
    public class DrawnCardDTO
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
    public class DrawResultForPlayerDTO
    {
        public List<DrawnCardDTO> DrawnCards { get; set; } = new();
        public int FatigueCount { get; set; }
        public int BaseFatigue { get; set; }
    }

    [Serializable]
    public class DrawResultForOpponentDTO
    {
        public string PlayerId { get; set; }
        public int CardsDrawnCount { get; set; }
        public int FatigueCount { get; set; }
        public int BaseFatigue { get; set; }
    }
}