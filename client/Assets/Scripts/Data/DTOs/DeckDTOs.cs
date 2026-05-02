using System;
using System.Collections.Generic;

namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class DeckDataDto
    {
        public List<DeckCardDto> Cards { get; set; } = new List<DeckCardDto>();
        public DeckChampionDto Champion { get; set; } = new DeckChampionDto();
    }

    [Serializable]
    public class DeckCardDto
    {
        public Guid DeckCardId { get; set; }
        public int Quantity { get; set; }
        public Guid CollectionCardId { get; set; }
        public string Rarity { get; set; } = "";
        public Guid CardId { get; set; }
        public string Name { get; set; } = "";
        public int? Hp { get; set; }
        public int? Attack { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; } = "";
        public string Picture { get; set; } = "";
        public string Extension { get; set; } = "";
        public string CardType { get; set; } = "";
        public int Price { get; set; }
        public List<string> Classes { get; set; } = new List<string>();
    }

    [Serializable]
    public class DeckChampionDto
    {
        public Guid ChampionID { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int HP { get; set; }
        public string Picture { get; set; } = "";
        public Guid FactionId { get; set; }
    }
}
