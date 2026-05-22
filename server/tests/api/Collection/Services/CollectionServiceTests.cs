using Microsoft.EntityFrameworkCore;
using Moq;
using VortexTCG.Api.Collection.DTOs;
using VortexTCG.Api.Collection.Providers;
using VortexTCG.Api.Collection.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using CollectionModel = VortexTCG.DataAccess.Models.Collection;
using CollectionCardModel = VortexTCG.DataAccess.Models.CollectionCard;
using CardModel = VortexTCG.DataAccess.Models.Card;
using UserModel = VortexTCG.DataAccess.Models.User;
using RarityEnum = VortexTCG.DataAccess.Models.Rarity;
using DeckModel = VortexTCG.DataAccess.Models.Deck;
using ChampionModel = VortexTCG.DataAccess.Models.Champion;
using FactionModel = VortexTCG.DataAccess.Models.Faction;
using FactionCardModel = VortexTCG.DataAccess.Models.FactionCard;
using ClassCardModel = VortexTCG.DataAccess.Models.ClassCard;
using ClassModel = VortexTCG.DataAccess.Models.Class;
using Xunit;

namespace VortexTCG.Tests.Api.Collection.Services
{
    public class CollectionServiceTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static CollectionService CreateService(VortexDbContext db)
        {
            CollectionProvider provider = new CollectionProvider(db);
            VortexTCG.Api.Deck.Providers.DeckProvider deckProvider = new VortexTCG.Api.Deck.Providers.DeckProvider(db);
            return new CollectionService(provider, deckProvider);
        }

        [Fact]
        public async Task Create_Returns400_WhenUserIdMissing()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);
            CollectionCreateDto input = new CollectionCreateDto { UserId = Guid.Empty };

            ResultDTO<CollectionDto> result = await service.CreateAsync(input);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);
            CollectionCreateDto input = new CollectionCreateDto { UserId = Guid.NewGuid() };

            ResultDTO<CollectionDto> result = await service.CreateAsync(input);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.NotEqual(Guid.Empty, result.data!.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);

            ResultDTO<CollectionDto> result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task GetById_ReturnsData()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<CollectionDto> dto = await service.GetByIdAsync(entity.Id);

            Assert.True(dto.success);
            Assert.Equal(entity.Id, dto.data!.Id);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);
            CollectionCreateDto input = new CollectionCreateDto { UserId = Guid.NewGuid() };

            ResultDTO<CollectionDto> result = await service.UpdateAsync(Guid.NewGuid(), input);

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);
            CollectionCreateDto input = new CollectionCreateDto { UserId = Guid.NewGuid() };

            ResultDTO<CollectionDto> result = await service.UpdateAsync(entity.Id, input);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal(entity.Id, result.data!.Id);
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);

            ResultDTO<bool> result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<bool> result = await service.DeleteAsync(entity.Id);

            Assert.True(result.success);
            Assert.Equal(204, result.statusCode);
        }

        [Fact]
        public async Task Update_Returns400_WhenUserIdEmpty()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);
            CollectionCreateDto input = new CollectionCreateDto { UserId = Guid.Empty };

            ResultDTO<CollectionDto> result = await service.UpdateAsync(entity.Id, input);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsEmpty_WhenNoCollections()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);

            ResultDTO<CollectionDto[]> result = await service.GetAllAsync();

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Empty(result.data!);
        }

        [Fact]
        public async Task GetAll_ReturnsAll_WhenCollectionsExist()
        {
            using VortexDbContext db = CreateDb();
            db.Collections.Add(new CollectionModel { Id = Guid.NewGuid() });
            db.Collections.Add(new CollectionModel { Id = Guid.NewGuid() });
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<CollectionDto[]> result = await service.GetAllAsync();

            Assert.True(result.success);
            Assert.Equal(2, result.data!.Length);
        }

        [Fact]
        public async Task GetCollectionByUserId_Returns400_WhenUserIdEmpty()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(Guid.Empty);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task GetCollectionByUserId_Returns404_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task GetCollectionByUserId_Returns200_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid(), User = user };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Empty(result.data!.Cards);
        }

        [Fact]
        public async Task GetCollectionByUserId_ReturnsCard_WhenCollectionHasCard()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CardModel card = new CardModel
            {
                Id = Guid.NewGuid(), Name = "Fireball", Price = 50,
                Hp = 3, Attack = 5, Cost = 2,
                Description = "desc", Picture = "img.png"
            };
            db.Cards.Add(card);
            CollectionModel collection = new CollectionModel
            {
                Id = Guid.NewGuid(),
                User = user,
                Cards = new List<CollectionCardModel>
                {
                    new CollectionCardModel { Id = Guid.NewGuid(), Card = card, Quantity = 2, Rarity = RarityEnum.EPIC }
                }
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Single(result.data!.Cards);
            UserCollectionCardDto dto = result.data.Cards[0];
            Assert.Equal(card.Id, dto.Card.Id);
            Assert.Equal("Fireball", dto.Card.Name);
            Assert.Equal(50, dto.Card.Price);
            Assert.Equal(3, dto.Card.Hp);
            Assert.Equal(5, dto.Card.Attack);
            Assert.Equal(2, dto.Card.Cost);
            Assert.Single(dto.OwnData);
            Assert.Equal(2, dto.OwnData[0].Number);
            Assert.Equal("EPIC", dto.OwnData[0].Rarity);
        }

        [Fact]
        public async Task GetCollectionByUserId_MapsNullHpAndAttackAsZero()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CardModel card = new CardModel
            {
                Id = Guid.NewGuid(), Name = "Shield", Price = 10,
                Hp = null, Attack = null, Cost = 1,
                Description = "desc", Picture = "img.png"
            };
            db.Cards.Add(card);
            CollectionModel collection = new CollectionModel
            {
                Id = Guid.NewGuid(),
                User = user,
                Cards = new List<CollectionCardModel>
                {
                    new CollectionCardModel { Id = Guid.NewGuid(), Card = card, Quantity = 1, Rarity = RarityEnum.NORMAL }
                }
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Single(result.data!.Cards);
            UserCollectionCardDto dto = result.data.Cards[0];
            Assert.Equal(0, dto.Card.Hp);
            Assert.Equal(0, dto.Card.Attack);
        }

        [Fact]
        public async Task GetCollectionByUserId_ReturnsAllCards_WhenMultipleCards()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CardModel card1 = new CardModel { Id = Guid.NewGuid(), Name = "Card1", Cost = 1, Description = "d", Picture = "p" };
            CardModel card2 = new CardModel { Id = Guid.NewGuid(), Name = "Card2", Cost = 2, Description = "d", Picture = "p" };
            db.Cards.AddRange(card1, card2);
            CollectionModel collection = new CollectionModel
            {
                Id = Guid.NewGuid(),
                User = user,
                Cards = new List<CollectionCardModel>
                {
                    new CollectionCardModel { Id = Guid.NewGuid(), Card = card1, Quantity = 1, Rarity = RarityEnum.NORMAL },
                    new CollectionCardModel { Id = Guid.NewGuid(), Card = card2, Quantity = 2, Rarity = RarityEnum.RARE }
                }
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Equal(2, result.data!.Cards.Count);
        }

        [Fact]
        public async Task Update_Returns500_WhenProviderUpdateFails()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();

            Mock<CollectionProvider> mockProvider = new Mock<CollectionProvider>(db) { CallBase = true };
            mockProvider.Setup(p => p.UpdateAsync(It.IsAny<CollectionModel>())).ReturnsAsync(false);
            CollectionService service = new CollectionService(
                mockProvider.Object,
                new VortexTCG.Api.Deck.Providers.DeckProvider(db));

            ResultDTO<CollectionDto> result = await service.UpdateAsync(entity.Id, new CollectionCreateDto { UserId = Guid.NewGuid() });

            Assert.False(result.success);
            Assert.Equal(500, result.statusCode);
            Assert.Contains("mise à jour", result.message);
        }

        private static (ChampionModel champion, FactionModel faction) SeedChampionAndFaction(
            VortexDbContext db, string picture = "hero.png")
        {
            FactionModel faction = new FactionModel
            {
                Id = Guid.NewGuid(), Label = "Test Faction",
                Currency = "Gold", Condition = "None",
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            };
            ChampionModel champion = new ChampionModel
            {
                Id = Guid.NewGuid(), Name = "Hero", Description = "desc",
                HP = 30, Picture = picture, FactionId = faction.Id,
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            };
            db.Factions.Add(faction);
            db.Champions.Add(champion);
            return (champion, faction);
        }

        [Fact]
        public async Task GetCollectionByUserId_PopulatesDecks_WhenUserHasDecks()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CollectionModel collection = new CollectionModel { Id = Guid.NewGuid(), User = user };
            db.Collections.Add(collection);
            (ChampionModel champion, FactionModel faction) = SeedChampionAndFaction(db, "hero.png");
            DeckModel deck = new DeckModel
            {
                Id = Guid.NewGuid(), UserId = user.Id, Label = "My Deck",
                ChampionId = champion.Id, FactionId = faction.Id,
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            };
            db.Decks.Add(deck);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Single(result.data!.Decks);
            UserCollectionDeckDto deckDto = result.data.Decks[0];
            Assert.Equal(deck.Id, deckDto.DeckId);
            Assert.Equal(champion.Id, deckDto.ChampionId);
            Assert.Equal(faction.Id, deckDto.FactionId);
            Assert.Equal("My Deck", deckDto.DeckName);
            Assert.Equal("hero.png", deckDto.ChampionImage);
        }

        [Fact]
        public async Task GetCollectionByUserId_UsesFallbackDeckName_WhenLabelIsEmpty()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CollectionModel collection = new CollectionModel { Id = Guid.NewGuid(), User = user };
            db.Collections.Add(collection);
            (ChampionModel champion, FactionModel faction) = SeedChampionAndFaction(db);
            DeckModel deck = new DeckModel
            {
                Id = Guid.NewGuid(), UserId = user.Id, Label = "",
                ChampionId = champion.Id, FactionId = faction.Id,
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            };
            db.Decks.Add(deck);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.Single(result.data!.Decks);
            Assert.Equal("Deck", result.data.Decks[0].DeckName);
        }

        [Fact]
        public async Task GetCollectionByUserId_ChampionImageIsEmpty_WhenChampionHasNoImage()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CollectionModel collection = new CollectionModel { Id = Guid.NewGuid(), User = user };
            db.Collections.Add(collection);
            (ChampionModel champion, FactionModel faction) = SeedChampionAndFaction(db, picture: "");
            DeckModel deck = new DeckModel
            {
                Id = Guid.NewGuid(), UserId = user.Id, Label = "No Pic Deck",
                ChampionId = champion.Id, FactionId = faction.Id,
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            };
            db.Decks.Add(deck);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.Single(result.data!.Decks);
            Assert.Equal(string.Empty, result.data.Decks[0].ChampionImage);
        }

        [Fact]
        public async Task GetCollectionByUserId_Returns200WithEmptyDecks_WhenDeckProviderThrows()
        {
            VortexDbContext db1 = VortexDbCoontextFactory.getInMemoryDbContext();
            VortexDbContext db2 = VortexDbCoontextFactory.getInMemoryDbContext();
            try
            {
                UserModel user = CreateTestUser(Guid.NewGuid());
                db1.Users.Add(user);
                CollectionModel collection = new CollectionModel { Id = Guid.NewGuid(), User = user };
                db1.Collections.Add(collection);
                await db1.SaveChangesAsync();

                db2.Dispose();
                CollectionProvider collectionProvider = new CollectionProvider(db1);
                VortexTCG.Api.Deck.Providers.DeckProvider deckProvider = new VortexTCG.Api.Deck.Providers.DeckProvider(db2);
                CollectionService service = new CollectionService(collectionProvider, deckProvider);

                ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

                Assert.True(result.success);
                Assert.Equal(200, result.statusCode);
                Assert.Empty(result.data!.Decks);
            }
            finally
            {
                db1.Dispose();
            }
        }

        [Fact]
        public async Task GetCollectionByUserId_FiltersOutCard_WhenCardNavigationIsNull()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CollectionModel collection = new CollectionModel { Id = Guid.NewGuid(), User = user };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();
            db.CollectionCards.Add(new CollectionCardModel
            {
                Id = Guid.NewGuid(),
                CardId = Guid.NewGuid(),
                CollectionId = collection.Id,
                Quantity = 1,
                Rarity = RarityEnum.NORMAL
            });
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Empty(result.data!.Cards);
        }

        [Fact]
        public async Task GetCollectionByUserId_MapsClasses_WhenCardHasClassCards()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CardModel card = new CardModel
            {
                Id = Guid.NewGuid(), Name = "Knight", Cost = 2,
                Description = "desc", Picture = "pic.png"
            };
            db.Cards.Add(card);
            ClassModel cls = new ClassModel
            {
                Id = Guid.NewGuid(), Label = "Warrior",
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            };
            db.Set<ClassModel>().Add(cls);
            db.Set<ClassCardModel>().Add(new ClassCardModel
            {
                Id = Guid.NewGuid(), CardId = card.Id, ClassId = cls.Id,
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            });
            CollectionModel collection = new CollectionModel
            {
                Id = Guid.NewGuid(), User = user,
                Cards = new List<CollectionCardModel>
                {
                    new CollectionCardModel { Id = Guid.NewGuid(), Card = card, Quantity = 1, Rarity = RarityEnum.NORMAL }
                }
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Single(result.data!.Cards);
            Assert.Single(result.data.Cards[0].Card.Class);
        }

        [Fact]
        public async Task GetCollectionByUserId_FiltersOutClassCard_WhenClassNavigationIsNull()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CardModel card = new CardModel
            {
                Id = Guid.NewGuid(), Name = "Mage", Cost = 3,
                Description = "desc", Picture = "pic.png"
            };
            db.Cards.Add(card);
            db.Set<ClassCardModel>().Add(new ClassCardModel
            {
                Id = Guid.NewGuid(), CardId = card.Id, ClassId = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            });
            CollectionModel collection = new CollectionModel
            {
                Id = Guid.NewGuid(), User = user,
                Cards = new List<CollectionCardModel>
                {
                    new CollectionCardModel { Id = Guid.NewGuid(), Card = card, Quantity = 1, Rarity = RarityEnum.NORMAL }
                }
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Single(result.data!.Cards);
            Assert.Empty(result.data.Cards[0].Card.Class);
        }

        [Fact]
        public async Task GetCollectionByUserId_MapsFactionIds_WhenCardHasFactionCards()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CardModel card = new CardModel
            {
                Id = Guid.NewGuid(), Name = "Archer", Cost = 1,
                Description = "desc", Picture = "pic.png"
            };
            db.Cards.Add(card);
            FactionModel faction = new FactionModel
            {
                Id = Guid.NewGuid(), Label = "Forest", Currency = "Wood", Condition = "None",
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            };
            db.Factions.Add(faction);
            db.FactionCards.Add(new FactionCardModel
            {
                Id = Guid.NewGuid(), CardId = card.Id, FactionId = faction.Id,
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            });
            CollectionModel collection = new CollectionModel
            {
                Id = Guid.NewGuid(), User = user,
                Cards = new List<CollectionCardModel>
                {
                    new CollectionCardModel { Id = Guid.NewGuid(), Card = card, Quantity = 1, Rarity = RarityEnum.NORMAL }
                }
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Single(result.data!.Cards);
            Guid factionId = Assert.Single(result.data.Cards[0].Card.Factions);
            Assert.Equal(faction.Id, factionId);
        }

        [Fact]
        public async Task GetCollectionByUserId_ReturnsEmptyFactions_WhenCardHasNoFactionCards()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CardModel card = new CardModel
            {
                Id = Guid.NewGuid(), Name = "Rogue", Cost = 2,
                Description = "desc", Picture = "pic.png"
            };
            db.Cards.Add(card);
            CollectionModel collection = new CollectionModel
            {
                Id = Guid.NewGuid(), User = user,
                Cards = new List<CollectionCardModel>
                {
                    new CollectionCardModel { Id = Guid.NewGuid(), Card = card, Quantity = 1, Rarity = RarityEnum.NORMAL }
                }
            };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Single(result.data!.Cards);
            Assert.Empty(result.data.Cards[0].Card.Factions);
        }

        [Fact]
        public async Task GetCollectionByUserId_KeepsDecksEmpty_WhenUserHasNoDecks()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CollectionModel collection = new CollectionModel { Id = Guid.NewGuid(), User = user };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Empty(result.data!.Decks);
        }

        [Fact]
        public async Task GetCollectionByUserId_ReturnsEmptyCards_WhenCollectionCardsIsNull()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            await db.SaveChangesAsync();

            CollectionModel collectionWithNullCards = new CollectionModel
            {
                Id = Guid.NewGuid(),
                User = user,
                Cards = null!
            };
            Mock<CollectionProvider> mockProvider = new Mock<CollectionProvider>(db) { CallBase = true };
            mockProvider.Setup(p => p.GetByUserIdAsync(user.Id)).ReturnsAsync(collectionWithNullCards);
            CollectionService service = new CollectionService(
                mockProvider.Object,
                new VortexTCG.Api.Deck.Providers.DeckProvider(db));

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Empty(result.data!.Cards);
        }

        [Fact]
        public async Task GetCollectionByUserId_ReturnsEmptyDecks_WhenDecksIsNull()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CollectionModel collection = new CollectionModel { Id = Guid.NewGuid(), User = user };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();

            CollectionProvider collectionProvider = new CollectionProvider(db);
            Mock<VortexTCG.Api.Deck.Providers.DeckProvider> mockDeckProvider =
                new Mock<VortexTCG.Api.Deck.Providers.DeckProvider>(db) { CallBase = true };
            mockDeckProvider
                .Setup(p => p.GetDecksByUserIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((List<DeckModel>)null!);
            CollectionService service = new CollectionService(collectionProvider, mockDeckProvider.Object);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Empty(result.data!.Decks);
        }

        [Fact]
        public async Task GetCollectionByUserId_ChampionImageIsEmpty_WhenChampionNavigationIsNull()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = CreateTestUser(Guid.NewGuid());
            db.Users.Add(user);
            CollectionModel collection = new CollectionModel { Id = Guid.NewGuid(), User = user };
            db.Collections.Add(collection);
            await db.SaveChangesAsync();

            DeckModel deckWithNullChampion = new DeckModel
            {
                Id = Guid.NewGuid(), UserId = user.Id, Label = "No Champion Deck",
                ChampionId = Guid.NewGuid(), FactionId = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test"
            };

            CollectionProvider collectionProvider = new CollectionProvider(db);
            Mock<VortexTCG.Api.Deck.Providers.DeckProvider> mockDeckProvider =
                new Mock<VortexTCG.Api.Deck.Providers.DeckProvider>(db) { CallBase = true };
            mockDeckProvider
                .Setup(p => p.GetDecksByUserIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<DeckModel> { deckWithNullChampion });
            CollectionService service = new CollectionService(collectionProvider, mockDeckProvider.Object);

            ResultDTO<UserCollectionDto> result = await service.GetCollectionByUserId(user.Id);

            Assert.True(result.success);
            Assert.Single(result.data!.Decks);
            Assert.Equal(string.Empty, result.data.Decks[0].ChampionImage);
        }

        private static UserModel CreateTestUser(Guid id) => new UserModel
        {
            Id = id,
            FirstName = "Test", LastName = "User",
            Username = $"user_{id:N}",
            Email = $"{id:N}@test.com",
            Password = "hash", Language = "fr",
            Role = VortexTCG.DataAccess.Models.Role.USER,
            Status = VortexTCG.DataAccess.Models.UserStatus.DISCONNECTED
        };
    }
}
