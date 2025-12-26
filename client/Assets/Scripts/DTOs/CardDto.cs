using System;
using System.Collections.Generic;

namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class CardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public int Hp { get; set; }
        public int Attack { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; } = "";
        public string CardType { get; set; } = "";
        public string Extension { get; set; } = "";
        public string Picture { get; set; } = "";
        public List<Guid> Factions { get; set; } = new List<Guid>();
    }
}
