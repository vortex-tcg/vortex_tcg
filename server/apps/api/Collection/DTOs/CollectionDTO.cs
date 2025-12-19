using System.ComponentModel.DataAnnotations;
using VortexTCG.Api.Card.DTOs;

namespace VortexTCG.Api.Collection.DTOs
{
    // DTO retourné par l'API
    public class CollectionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }

    // DTO utilisé pour la création
    public class CollectionCreateDto
    {
        public required Guid UserId { get; set; }
    }

    public class UserCollectionDeckDto
    {
        public required Guid DeckId { get; set; }

        public string DeckName { get; set; } = "";
        
        public string ChampionImage { get; set; } = "";
    }

    public class UserCollectionCardOwnDto
    {
        public int Number { get; set; } = 0;
        public string Rarity { get; set; } = default!;
    }

    public class UserCollectionCardDto
    {
        public CardDto Card { get; set; }
        public List<UserCollectionCardOwnDto> OwnData { get; set; } = new List<UserCollectionCardOwnDto>();
    }

    public class UserCollectionFactionDto
    {
        public Guid FactionId { get; set; }
        public string FactionName { get; set; } = default!;
        public string FactionImage { get; set; } = default!;
    }

    public class UserCollectionDto
    {
        public List<UserCollectionDeckDto> Decks { get; set; } = new List<UserCollectionDeckDto>();
        public List<UserCollectionCardDto> Cards { get; set; } = new List<UserCollectionCardDto>();
        public List<UserCollectionFactionDto> Faction { get; set; } = new List<UserCollectionFactionDto>();
    }
}
