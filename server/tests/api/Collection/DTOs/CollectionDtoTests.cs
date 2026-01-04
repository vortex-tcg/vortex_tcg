using VortexTCG.Api.Collection.DTOs;
using VortexTCG.Api.Card.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Collection.DTOs
{
    public class CollectionDtoTests
    {
        [Fact]
        public void CollectionDto_CanSetAndGetProperties()
        {
            Guid id = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            CollectionDto dto = new CollectionDto
            {
                Id = id,
                UserId = userId
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal(userId, dto.UserId);
        }

        [Fact]
        public void CollectionCreateDto_CanSetUserId()
        {
            Guid userId = Guid.NewGuid();
            CollectionCreateDto dto = new CollectionCreateDto { UserId = userId };

            Assert.Equal(userId, dto.UserId);
        }

        [Fact]
        public void UserCollectionDeckDto_CanSetAndGetProperties()
        {
            Guid deckId = Guid.NewGuid();
            UserCollectionDeckDto dto = new UserCollectionDeckDto
            {
                DeckId = deckId,
                DeckName = "MyDeck",
                ChampionImage = "champ.jpg"
            };

            Assert.Equal(deckId, dto.DeckId);
            Assert.Equal("MyDeck", dto.DeckName);
            Assert.Equal("champ.jpg", dto.ChampionImage);
        }

        [Fact]
        public void UserCollectionCardOwnDto_CanSetAndGetProperties()
        {
            UserCollectionCardOwnDto dto = new UserCollectionCardOwnDto
            {
                Number = 3,
                Rarity = "Rare"
            };

            Assert.Equal(3, dto.Number);
            Assert.Equal("Rare", dto.Rarity);
        }

        [Fact]
        public void UserCollectionCardOwnDto_DefaultNumber()
        {
            UserCollectionCardOwnDto dto = new UserCollectionCardOwnDto();
            Assert.Equal(0, dto.Number);
        }

        [Fact]
        public void UserCollectionCardDto_CanSetAndGetProperties()
        {
            CardDto card = new CardDto { Id = Guid.NewGuid(), Name = "TestCard" };
            List<UserCollectionCardOwnDto> ownData = new List<UserCollectionCardOwnDto>
            {
                new UserCollectionCardOwnDto { Number = 2, Rarity = "Common" }
            };

            UserCollectionCardDto dto = new UserCollectionCardDto
            {
                Card = card,
                OwnData = ownData
            };

            Assert.NotNull(dto.Card);
            Assert.Equal("TestCard", dto.Card.Name);
            Assert.Single(dto.OwnData);
        }

        [Fact]
        public void UserCollectionCardDto_DefaultOwnData()
        {
            UserCollectionCardDto dto = new UserCollectionCardDto
            {
                Card = new CardDto()
            };

            Assert.NotNull(dto.OwnData);
            Assert.Empty(dto.OwnData);
        }

        [Fact]
        public void UserCollectionFactionDto_CanSetAndGetProperties()
        {
            Guid factionId = Guid.NewGuid();
            UserCollectionFactionDto dto = new UserCollectionFactionDto
            {
                FactionId = factionId,
                FactionName = "Empire",
                FactionImage = "empire.jpg"
            };

            Assert.Equal(factionId, dto.FactionId);
            Assert.Equal("Empire", dto.FactionName);
            Assert.Equal("empire.jpg", dto.FactionImage);
        }

        [Fact]
        public void UserCollectionDto_CanSetAndGetProperties()
        {
            List<UserCollectionDeckDto> decks = new List<UserCollectionDeckDto>
            {
                new UserCollectionDeckDto { DeckId = Guid.NewGuid(), DeckName = "Deck1" }
            };
            List<UserCollectionFactionDto> factions = new List<UserCollectionFactionDto>
            {
                new UserCollectionFactionDto { FactionId = Guid.NewGuid(), FactionName = "Faction1" }
            };

            UserCollectionDto dto = new UserCollectionDto
            {
                Decks = decks,
                Cards = new List<UserCollectionCardDto>(),
                Faction = factions
            };

            Assert.Single(dto.Decks);
            Assert.Empty(dto.Cards);
            Assert.Single(dto.Faction);
        }

        [Fact]
        public void UserCollectionDto_DefaultEmptyCollections()
        {
            UserCollectionDto dto = new UserCollectionDto();

            Assert.NotNull(dto.Decks);
            Assert.NotNull(dto.Cards);
            Assert.NotNull(dto.Faction);
            Assert.Empty(dto.Decks);
            Assert.Empty(dto.Cards);
            Assert.Empty(dto.Faction);
        }
    }
}
