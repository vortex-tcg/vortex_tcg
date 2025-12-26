using System;
using System.Collections.Generic;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class CollectionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }

    [Serializable]
    public class CollectionCreateDto
    {
        public Guid UserId { get; set; }
    }

    [Serializable]
    public class UserCollectionDeckDto
    {
        public Guid DeckId { get; set; }

        public string DeckName { get; set; } = "";
        
        public string ChampionImage { get; set; } = "";
    }

    [Serializable]
    public class UserCollectionCardOwnDto
    {
        public int Number { get; set; } = 0;
        public string Rarity { get; set; } = default!;
    }
    [Serializable] 
    public class UserCollectionCardDto
    {
        public CardDto Card { get; set; }
        public List<UserCollectionCardOwnDto> OwnData { get; set; } = new List<UserCollectionCardOwnDto>();
    }

    [Serializable]
    public class UserCollectionFactionDto
    {
        public Guid FactionId { get; set; }
        public string FactionName { get; set; } = default!;
        public string FactionImage { get; set; } = default!;
    }

    [Serializable]
    public class UserCollectionDto
    {
        public List<UserCollectionDeckDto> Decks { get; set; } = new List<UserCollectionDeckDto>();
        public List<UserCollectionCardDto> Cards { get; set; } = new List<UserCollectionCardDto>();
        public List<UserCollectionFactionDto> Faction { get; set; } = new List<UserCollectionFactionDto>();
    }
}
