using System;
using System.Collections.Generic;

namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class CardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public int Price { get; set; }
        public int Hp { get; set; }
        public int Attack { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; } = default!;
        public string Picture { get; set; } = "";
        public string Extension { get; set; } = default!;
        public string CardType { get; set; } = default!;
        public List<string> Class { get; set; } = new();
        public List<Guid> Factions { get; set; } = new();
    }
}
