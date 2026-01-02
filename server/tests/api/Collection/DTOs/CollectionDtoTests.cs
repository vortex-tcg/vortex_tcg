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
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var dto = new CollectionDto
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
            var userId = Guid.NewGuid();
            var dto = new CollectionCreateDto { UserId = userId };

            Assert.Equal(userId, dto.UserId);
        }

        [Fact]
        public void UserCollectionDeckDto_CanSetAndGetProperties()
        {
            var deckId = Guid.NewGuid();
            var dto = new UserCollectionDeckDto
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
            var dto = new UserCollectionCardOwnDto
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
            var dto = new UserCollectionCardOwnDto();
            Assert.Equal(0, dto.Number);
        }

        [Fact]
        public void UserCollectionCardDto_CanSetAndGetProperties()
        {
            var card = new CardDto { Id = Guid.NewGuid(), Name = "TestCard" };
            var ownData = new List<UserCollectionCardOwnDto>
            {
                new UserCollectionCardOwnDto { Number = 2, Rarity = "Common" }
            };

            var dto = new UserCollectionCardDto
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
            var dto = new UserCollectionCardDto
            {
                Card = new CardDto()
            };

            Assert.NotNull(dto.OwnData);
            Assert.Empty(dto.OwnData);
        }

        [Fact]
        public void UserCollectionFactionDto_CanSetAndGetProperties()
        {
            var factionId = Guid.NewGuid();
            var dto = new UserCollectionFactionDto
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
            var decks = new List<UserCollectionDeckDto>
            {
                new UserCollectionDeckDto { DeckId = Guid.NewGuid(), DeckName = "Deck1" }
            };
            var factions = new List<UserCollectionFactionDto>
            {
                new UserCollectionFactionDto { FactionId = Guid.NewGuid(), FactionName = "Faction1" }
            };

            var dto = new UserCollectionDto
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
            var dto = new UserCollectionDto();

            Assert.NotNull(dto.Decks);
            Assert.NotNull(dto.Cards);
            Assert.NotNull(dto.Faction);
            Assert.Empty(dto.Decks);
            Assert.Empty(dto.Cards);
            Assert.Empty(dto.Faction);
        }
    }
}
