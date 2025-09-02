using Xunit;
using CollectionCards.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Collection.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardsController.Tests
{
    public class TestCardsController
    {
        private DbContextOptions<VortexDbContext> ContextOptions()
        {
            // Use a unique name for each test run to ensure isolation
            return new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        // GET /api/Cards
        [Fact]
        public async Task GetCards_ShouldReturnCardList()
        {
            // Arrange
            var options = ContextOptions();

            // Envoyer des données de test dans la base de données en mémoire
            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.Add(new Card { Id = 1, Name = "Test Card", Hp = 10, Attack = 5, Cost = 3, Description = "A test card", Picture = "test.png", Effect_active = 0, CardTypeId = 1, RarityId = 1, ExtensionId = 1 });
                await db.SaveChangesAsync();
            }

            // Act
            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.GetCards();

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var cards = Assert.IsAssignableFrom<IEnumerable<CardDTO>>(okResult.Value);

                Assert.NotNull(cards);
                Assert.Single(cards);

                var card = cards.First();
                Assert.Equal("Test Card", card.Name);
                Assert.Equal(10, card.Hp);
            }
        }

        [Fact]
        public async Task GetCards_ShouldReturnEmptyList_WhenNoCardsExist()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.GetCards();

                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var cards = Assert.IsAssignableFrom<IEnumerable<CardDTO>>(okResult.Value);

                Assert.NotNull(cards);
                Assert.Empty(cards);
            }
        }

        // GET /api/Cards/{id}
        [Fact]
        public async Task GetCardById_ShouldReturnCard()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.Add(new Card { Id = 1, Name = "Test Card", Hp = 10, Attack = 5, Cost = 3, Description = "A test card", Picture = "test.png", Effect_active = 0, CardTypeId = 1, RarityId = 1, ExtensionId = 1 });
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.GetCard(1);

                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var card = Assert.IsType<CardDTO>(okResult.Value);

                Assert.NotNull(card);
                Assert.Equal("Test Card", card.Name);
                Assert.Equal(10, card.Hp);
            }
        }

        [Fact]
        public async Task GetCardById_ShouldReturnNotFound()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.GetCard(10); // ID inexistant

                Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            }
        }

        // POST /api/Cards
        [Fact]
        public async Task CreateCard_WithValidData_ReturnsCreatedWithCard()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                await db.SaveChangesAsync();
            }

            var newCard = new CreateCardDTO
            {
                Name = "New Card",
                Hp = 20,
                Attack = 10,
                Cost = 5,
                Description = "A new test card",
                Picture = "newcard.png",
                Effect_active = 1,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            };

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.CreateCard(newCard);

                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
                var createdCard = Assert.IsType<CardDTO>(createdAtActionResult.Value);

                Assert.NotNull(createdCard);
                Assert.Equal("New Card", createdCard.Name);
                Assert.Equal(20, createdCard.Hp);
            }
        }

        [Fact]
        public async Task CreateCard_WithInvalidData_ReturnsBadRequest()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                await db.SaveChangesAsync();
            }

            var invalidCard = new CreateCardDTO
            {
                Name = "",
                Hp = -10,
                Attack = 5,
                Cost = 5,
                Description = "A new test card",
                Picture = "newcard.png",
                Effect_active = 1,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            };

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                controller.ModelState.AddModelError("Name", "Le nom est requis.");
                controller.ModelState.AddModelError("Hp", "Hp doit être entre 0 et 100.");

                var actionResult = await controller.CreateCard(invalidCard);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
                var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);

                Assert.True(modelState.ContainsKey("Name"));
                Assert.True(modelState.ContainsKey("Hp"));
            }
        }

        [Fact]
        public async Task CreateCard_WithInvalidForeignKey_ReturnsBadRequest()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var newCard = new CreateCardDTO { Name = "Test", CardTypeId = 99, RarityId = 99, ExtensionId = 99 };
                var actionResult = await controller.CreateCard(newCard);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
                Assert.NotNull(badRequestResult.Value);
            }
        }

        // PUT /api/Cards/{id}
        [Fact]
        public async Task UpdateCard_WithValidData_ReturnUpdatedCard()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.Add(new Card { Id = 1, Name = "Test Card", Hp = 10, Attack = 5, Cost = 3, Description = "A test card", Picture = "test.png", Effect_active = 0, CardTypeId = 1, RarityId = 1, ExtensionId = 1 });
                await db.SaveChangesAsync();
            }

            var updateCard = new UpdateCardDTO
            {
                Name = "Updated Card",
                Hp = 30,
                Attack = 15,
                Cost = 7,
                Description = "An updated test card",
                Picture = "updatedcard.png",
                Effect_active = 2,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            };

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.UpdateCard(1, updateCard);

                Assert.IsType<NoContentResult>(actionResult);

                var updatedCard = await db.Cards.FindAsync(1);
                Assert.Equal("Updated Card", updatedCard.Name);
                Assert.Equal(30, updatedCard.Hp);
            }
        }

        [Fact]
        public async Task UpdateCard_WithNonExistentId_ReturnsNotFound()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.Add(new Card { Id = 1, Name = "Test Card", Hp = 10, Attack = 5, Cost = 3, Description = "A test card", Picture = "test.png", Effect_active = 0, CardTypeId = 1, RarityId = 1, ExtensionId = 1 });
                await db.SaveChangesAsync();
            }

            var updateCard = new UpdateCardDTO
            {
                Name = "Updated Card",
                Hp = 30,
                Attack = 15,
                Cost = 7,
                Description = "An updated test card",
                Picture = "updatedcard.png",
                Effect_active = 2,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            };

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.UpdateCard(5, updateCard);

                Assert.IsType<NotFoundObjectResult>(actionResult);

                var unchangedCard = await db.Cards.FindAsync(1);
                Assert.Equal("Test Card", unchangedCard.Name);
                Assert.Equal(10, unchangedCard.Hp);
            }
        }

        [Fact]
        public async Task UpdateCard_WithInvalidData_ReturnBadRequest()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.Add(new Card { Id = 1, Name = "Test Card", Hp = 10, Attack = 5, Cost = 3, Description = "A test card", Picture = "test.png", Effect_active = 0, CardTypeId = 1, RarityId = 1, ExtensionId = 1 });
                await db.SaveChangesAsync();
            }

            var invalidUpdateCard = new UpdateCardDTO
            {
                Name = "",
                Hp = -30,
                Attack = 15,
                Cost = 7,
                Description = "An updated test card",
                Picture = "",
                Effect_active = 2,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            };

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                controller.ModelState.AddModelError("Name", "Le nom est requis.");
                controller.ModelState.AddModelError("Hp", "Hp doit être entre 0 et 100.");
                controller.ModelState.AddModelError("Picture", "Picture doit être une URL valide.");

                var actionResult = await controller.UpdateCard(1, invalidUpdateCard);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
                var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);

                Assert.True(modelState.ContainsKey("Name"));
                Assert.True(modelState.ContainsKey("Hp"));
                Assert.True(modelState.ContainsKey("Picture"));

                var unchangedCard = await db.Cards.FindAsync(1);
                Assert.Equal("Test Card", unchangedCard.Name);
                Assert.Equal(10, unchangedCard.Hp);
                Assert.Equal("test.png", unchangedCard.Picture);
            }
        }

        [Fact]
        public async Task UpdateCard_WithInvalidForeignKey_ReturnsBadRequest()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.Add(new Card { Id = 1, Name = "Test Card", Hp = 10, Attack = 5, Cost = 3, Description = "A test card", Picture = "test.png", Effect_active = 0, CardTypeId = 1, RarityId = 1, ExtensionId = 1 });
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var updatedCard = new UpdateCardDTO { Name = "Updated", CardTypeId = 99, RarityId = 99, ExtensionId = 99 };
                var result = await controller.UpdateCard(1, updatedCard);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.NotNull(badRequestResult.Value);
            }
        }

        // DELETE /api/Cards/{id}
        [Fact]
        public async Task DeleteCard_WithValidId_ReturnsNoContent()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.Add(new Card { Id = 1, Name = "Test Card", Hp = 10, Attack = 5, Cost = 3, Description = "A test card", Picture = "test.png", Effect_active = 0, CardTypeId = 1, RarityId = 1, ExtensionId = 1 });
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.DeleteCard(1);

                Assert.IsType<NoContentResult>(actionResult);

                var deletedCard = await db.Cards.FindAsync(1);
                Assert.Null(deletedCard);
            }
        }

        [Fact]
        public async Task DeleteCard_WithNonExistentId_ReturnsNotFound()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.Add(new Card { Id = 1, Name = "Test Card", Hp = 10, Attack = 5, Cost = 3, Description = "A test card", Picture = "test.png", Effect_active = 0, CardTypeId = 1, RarityId = 1, ExtensionId = 1 });
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.DeleteCard(5);

                Assert.IsType<NotFoundObjectResult>(actionResult);

                var existingCard = await db.Cards.FindAsync(1);
                Assert.NotNull(existingCard);
                Assert.Equal("Test Card", existingCard.Name);
            }
        }

        // GET /api/Cards/search?name={name}
        [Fact]
        public async Task SearchCards_ByName_ReturnsMatchingCards()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.AddRange(
                    new Card { Id = 1, Name = "Dragon", Hp = 50, Attack = 30, Cost = 10, Description = "A fierce dragon", Picture = "dragon.png", Effect_active = 1, CardTypeId = 1, RarityId = 1, ExtensionId = 1 },
                    new Card { Id = 2, Name = "Knight", Hp = 40, Attack = 20, Cost = 8, Description = "A brave knight", Picture = "knight.png", Effect_active = 2, CardTypeId = 1, RarityId = 1, ExtensionId = 1 },
                    new Card { Id = 3, Name = "Dragon Rider", Hp = 45, Attack = 25, Cost = 9, Description = "Rides a dragon", Picture = "dragonrider.png", Effect_active = 3, CardTypeId = 1, RarityId = 1, ExtensionId = 1 }
                );
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.SearchCards(name: "Dragon", cardTypeId: null, rarityId: null);

                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var cards = Assert.IsAssignableFrom<IEnumerable<CardDTO>>(okResult.Value);

                Assert.NotNull(cards);
                Assert.Equal(2, cards.Count());
                Assert.All(cards, c => Assert.Contains("Dragon", c.Name));
            }
        }

        [Fact]
        public async Task SearchCards_WithMultipleParameters_ReturnsFilteredCards()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.AddRange(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.AddRange(new Rarity { Id = 1, Label = "Common" }, new Rarity { Id = 2, Label = "Rare" });
                db.Extensions.AddRange(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.AddRange(
                    new Card { Id = 1, Name = "Dragon Rider", Hp = 50, Attack = 30, Cost = 10, Description = "A fierce dragon", Picture = "dragon.png", Effect_active = 1, CardTypeId = 1, RarityId = 2, ExtensionId = 1 },
                    new Card { Id = 2, Name = "Dragon", Hp = 40, Attack = 20, Cost = 8, Description = "A brave knight", Picture = "knight.png", Effect_active = 2, CardTypeId = 1, RarityId = 1, ExtensionId = 1 }
                );
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.SearchCards(name: "Dragon", cardTypeId: 1, rarityId: 2);

                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var cards = Assert.IsAssignableFrom<IEnumerable<CardDTO>>(okResult.Value);

                Assert.NotNull(cards);
                var cardList = cards.ToList();
                Assert.Single(cardList); // Ensure only one card is returned
                Assert.Equal("Dragon Rider", cardList.First().Name);
            }
        }

        [Fact]
        public async Task SearchCards_WithNoMatches_ReturnsEmptyList()
        {
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.AddRange(
                    new Card { Id = 1, Name = "Dragon", Hp = 50, Attack = 30, Cost = 10, Description = "A fierce dragon", Picture = "dragon.png", Effect_active = 1, CardTypeId = 1, RarityId = 1, ExtensionId = 1 },
                    new Card { Id = 2, Name = "Knight", Hp = 40, Attack = 20, Cost = 8, Description = "A brave knight", Picture = "knight.png", Effect_active = 2, CardTypeId = 1, RarityId = 1, ExtensionId = 1 }
                );
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.SearchCards(name: "Wizard", cardTypeId: null, rarityId: null);

                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var cards = Assert.IsAssignableFrom<IEnumerable<CardDTO>>(okResult.Value);

                Assert.NotNull(cards);
                Assert.Empty(cards);
            }
        }

        [Fact]
        public async Task SearchCards_WithNoParameters_ReturnsAllCards()
        { 
            var options = ContextOptions();

            using (var db = new VortexDbContext(options))
            {
                db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
                db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
                db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
                db.Cards.AddRange(
                    new Card { Id = 1, Name = "Dragon", Hp = 50, Attack = 30, Cost = 10, Description = "A fierce dragon", Picture = "dragon.png", Effect_active = 1, CardTypeId = 1, RarityId = 1, ExtensionId = 1 },
                    new Card { Id = 2, Name = "Knight", Hp = 40, Attack = 20, Cost = 8, Description = "A brave knight", Picture = "knight.png", Effect_active = 2, CardTypeId = 1, RarityId = 1, ExtensionId = 1 }
                );
                await db.SaveChangesAsync();
            }

            using (var db = new VortexDbContext(options))
            {
                var controller = new ControllerCards(db);
                var actionResult = await controller.SearchCards(name: null, cardTypeId: null, rarityId: null);

                var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
                var cards = Assert.IsAssignableFrom<IEnumerable<CardDTO>>(okResult.Value);

                Assert.NotNull(cards);
                Assert.Equal(2, cards.Count());
            }
        }
    }
}