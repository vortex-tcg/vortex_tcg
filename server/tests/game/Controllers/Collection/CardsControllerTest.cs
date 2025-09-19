using Xunit;
using VortexTCG.Cards.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Cards.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// Classe de tests unitaires pour le contrôleur <see cref="CardsController"/>.
    /// </summary>
    public class CardsControllerTest
    {
        /// <summary>
        /// Crée un contexte de base de données en mémoire pour les tests.
        /// </summary>
        /// <returns>Une instance de <see cref="VortexDbContext"/> configurée pour utiliser une base de données en mémoire.</returns>
        private VortexDbContext get_in_memory_db_context()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VortexDbContext(options);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.GetCards"/> retourne toutes les cartes.
        /// </summary>
        [Fact]
        public async Task getCardsShouldReturnCardList()
        {
            // Arrange
            var db = get_in_memory_db_context();

            db.Cards.AddRange(new List<Card>
            {
                new Card
                {
                    Id = 1,
                    Name = "Card 1",
                    Hp = 10,
                    Attack = 5,
                    Cost = 3,
                    Description = "First card",
                    Picture = "card1.png",
                    Effect_active = 0,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                },
                new Card
                {
                    Id = 2,
                    Name = "Card 2",
                    Hp = 20,
                    Attack = 10,
                    Cost = 6,
                    Description = "Second card",
                    Picture = "card2.png",
                    Effect_active = 1,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                }
            });
            await db.SaveChangesAsync();

            // Act
            var controller = new CardsController(db);
            var result = await controller.getCards();
            var ok_result = Assert.IsType<OkObjectResult>(result.Result);
            var cards = Assert.IsType<List<CardDTO>>(ok_result.Value);
            Assert.Equal(2, cards.Count);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.GetCards"/> retourne une liste vide lorsque aucune carte n'existe.
        /// </summary>
        [Fact]
        public async Task getCards_shouldReturnNotFound_whenNoCardsExist()
        {
            var db = get_in_memory_db_context();

            var controller = new CardsController(db);
            var result = await controller.getCards();
            var ok_result = Assert.IsType<OkObjectResult>(result.Result);
            var payload = ok_result.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Aucune carte trouvée", payload);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.GetCard"/> retourne une carte spécifique par son identifiant.
        /// </summary>
        [Fact]
        public async Task getCardById_shouldReturnCard()
        {
            var db = get_in_memory_db_context();

            db.Cards.Add(
                new Card
                {
                    Id = 1,
                    Name = "Test Card",
                    Hp = 10,
                    Attack = 5,
                    Cost = 3,
                    Description = "A test card",
                    Picture = "test.png",
                    Effect_active = 0,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                });
            await db.SaveChangesAsync();

            var controller = new CardsController(db);
            var actionResult = await controller.getCard(1);
            var ok_result = Assert.IsType<OkObjectResult>(actionResult.Result);
            var card = Assert.IsType<CardDTO>(ok_result.Value);
            Assert.NotNull(card);
            Assert.Equal("Test Card", card.name);
            Assert.Equal(10, card.hp);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.GetCard"/> retourne une erreur 404 lorsque l'identifiant est invalide.
        /// </summary>
        [Fact]
        public async Task getCardById_shouldReturnNotFound()
        {
            var db = get_in_memory_db_context();

            var controller = new CardsController(db);
            var actionResult = await controller.getCard(1);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            var payload = notFoundResult.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Carte introuvable", payload);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.CreateCard"/> crée une carte valide et retourne ses détails.
        /// </summary>
        [Fact]
        public async Task createCard_withValidData_returnsCreatedWithCard()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            await db.SaveChangesAsync();

            var newCard = new CreateCardDTO
            {
                name = "New Card",
                hp = 20,
                attack = 10,
                cost = 5,
                description = "A new test card",
                picture = "newcard.png",
                effect_active = 1,
                card_type_id = 1,
                rarity_id = 1,
                extension_id = 1
            };

            var controller = new CardsController(db);
            var actionResult = await controller.createCard(newCard);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var created_card = Assert.IsType<CardDTO>(createdAtActionResult.Value);

            Assert.NotNull(created_card);
            Assert.Equal("New Card", created_card.name);
            Assert.Equal(20, created_card.hp);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.CreateCard"/> retourne une erreur 400 pour des données invalides.
        /// </summary>
        [Fact]
        public async Task createCard_withInvalidData_returnsBadRequest()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            await db.SaveChangesAsync();

            var invalidCard = new CreateCardDTO
            {
                name = "",
                hp = -10,
                attack = 5,
                cost = 5,
                description = "A new test card",
                picture = "newcard.png",
                effect_active = 1,
                card_type_id = 1,
                rarity_id = 1,
                extension_id = 1
            };

            var controller = new CardsController(db);
            controller.ModelState.AddModelError("name", "Le nom est requis.");
            controller.ModelState.AddModelError("hp", "Hp doit être entre 0 et 100.");

            var actionResult = await controller.createCard(invalidCard);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);

            Assert.True(modelState.ContainsKey("name"));
            var nameErrors = modelState["name"] as string[];
            Assert.NotNull(nameErrors);
            Assert.Contains("Le nom est requis.", nameErrors);

            Assert.True(modelState.ContainsKey("hp"));
            var hpErrors = modelState["hp"] as string[];
            Assert.NotNull(hpErrors);
            Assert.Contains("Hp doit être entre 0 et 100.", hpErrors);

        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.CreateCard"/> retourne une erreur 400 pour des clés étrangères invalides.
        /// </summary>
        [Fact]
        public async Task createCard_withInvalidForeignKey_returnsBadRequest()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            await db.SaveChangesAsync();

            var newCard = new CreateCardDTO
            {
                name = "Test",
                card_type_id = 99,
                rarity_id = 99,
                extension_id = 99
            };

            var controller = new CardsController(db);
            var actionResult = await controller.createCard(newCard);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var payload = badRequestResult.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("CardType, Rarity ou Extension invalide", payload);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.UpdateCard"/> met à jour une carte existante avec des données valides.
        /// </summary>
        [Fact]
        public async Task updateCard_withValidData_returnUpdatedCard()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            db.Cards.Add(new Card
            {
                Id = 1,
                Name = "Test Card",
                Hp = 10,
                Attack = 5,
                Cost = 3,
                Description = "A test card",
                Picture = "test.png",
                Effect_active = 0,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            });
            await db.SaveChangesAsync();


            var updateCard = new UpdateCardDTO
            {
                name = "Updated Card",
                hp = 30,
                attack = 15,
                cost = 7,
                description = "An updated test card",
                picture = "updatedcard.png",
                effect_active = 2,
                card_type_id = 1,
                rarity_id = 1,
                extension_id = 1
            };

            var controller = new CardsController(db);
            var actionResult = await controller.updateCard(1, updateCard);

            Assert.IsType<NoContentResult>(actionResult);

            var updatedCard = await db.Cards.FindAsync(1);
            Assert.NotNull(updatedCard);
            Assert.Equal("Updated Card", updatedCard.Name);
            Assert.Equal(30, updatedCard.Hp);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.UpdateCard"/> retourne une erreur 404 pour un identifiant inexistant.
        /// </summary>
        [Fact]
        public async Task updateCard_withNonExistentId_returnsNotFound()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            db.Cards.Add(new Card
            {
                Id = 1,
                Name = "Test Card",
                Hp = 10,
                Attack = 5,
                Cost = 3,
                Description = "A test card",
                Picture = "test.png",
                Effect_active = 0,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            });
            await db.SaveChangesAsync();


            var updateCard = new UpdateCardDTO
            {
                name = "Updated Card",
                hp = 30,
                attack = 15,
                cost = 7,
                description = "An updated test card",
                picture = "updatedcard.png",
                effect_active = 2,
                card_type_id = 1,
                rarity_id = 1,
                extension_id = 1
            };

            var controller = new CardsController(db);
            var actionResult = await controller.updateCard(5, updateCard);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            var payload = notFoundResult.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Carte introuvable", payload);

            var unchangedCard = await db.Cards.FindAsync(1);
            Assert.NotNull(unchangedCard);
            Assert.Equal("Test Card", unchangedCard.Name);
            Assert.Equal(10, unchangedCard.Hp);

        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.UpdateCard"/> retourne une erreur 400 pour des données invalides.
        /// </summary>
        [Fact]
        public async Task updateCard_withInvalidData_returnBadRequest()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            db.Cards.Add(new Card
            {
                Id = 1,
                Name = "Test Card",
                Hp = 10,
                Attack = 5,
                Cost = 3,
                Description = "A test card",
                Picture = "test.png",
                Effect_active = 0,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            });
            await db.SaveChangesAsync();


            var invalidUpdateCard = new UpdateCardDTO
            {
                name = "",
                hp = -30,
                attack = 15,
                cost = 7,
                description = "An updated test card",
                picture = "",
                effect_active = 2,
                card_type_id = 1,
                rarity_id = 1,
                extension_id = 1
            };

            var controller = new CardsController(db);
            controller.ModelState.AddModelError("name", "Le nom est requis.");
            controller.ModelState.AddModelError("picture", "Picture doit être une URL valide.");

            var actionResult = await controller.updateCard(1, invalidUpdateCard);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);

            Assert.True(modelState.ContainsKey("name"));
            var nameErrors = modelState["name"] as string[];
            Assert.NotNull(nameErrors);
            Assert.Contains("Le nom est requis.", nameErrors);

            Assert.True(modelState.ContainsKey("picture"));
            var pictureErrors = modelState["picture"] as string[];
            Assert.NotNull(pictureErrors);
            Assert.Contains("Picture doit être une URL valide.", pictureErrors);

            var unchangedCard = await db.Cards.FindAsync(1);
            Assert.NotNull(unchangedCard);
            Assert.Equal("Test Card", unchangedCard.Name);
            Assert.Equal("test.png", unchangedCard.Picture);

        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.UpdateCard"/> retourne une erreur 400 pour des clés étrangères invalides.
        /// </summary>
        [Fact]
        public async Task updateCard_withInvalidForeignKey_returnsBadRequest()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            db.Cards.Add(new Card
            {
                Id = 1,
                Name = "Test Card",
                Hp = 10,
                Attack = 5,
                Cost = 3,
                Description = "A test card",
                Picture = "test.png",
                Effect_active = 0,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            });
            await db.SaveChangesAsync();

            var controller = new CardsController(db);
            var updatedCard = new UpdateCardDTO
            {
                name = "Updated",
                card_type_id = 99,
                rarity_id = 99,
                extension_id = 99
            };

            var result = await controller.updateCard(1, updatedCard);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
            var payload = badRequestResult.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("CardType, Rarity ou Extension invalide", payload);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.DeleteCard"/> supprime une carte existante avec un identifiant valide.
        /// </summary>
        [Fact]
        public async Task deleteCard_withValidId_returnsNoContent()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            db.Cards.Add(new Card
            {
                Id = 1,
                Name = "Test Card",
                Hp = 10,
                Attack = 5,
                Cost = 3,
                Description = "A test card",
                Picture = "test.png",
                Effect_active = 0,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            });
            await db.SaveChangesAsync();

            var controller = new CardsController(db);
            var actionResult = await controller.deleteCard(1);

            Assert.IsType<NoContentResult>(actionResult);

            var deletedCard = await db.Cards.FindAsync(1);
            Assert.Null(deletedCard);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.DeleteCard"/> retourne une erreur 404 pour un identifiant inexistant.
        /// </summary>
        [Fact]
        public async Task deleteCard_withNonExistentId_returnsNotFound()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            db.Cards.Add(new Card
            {
                Id = 1,
                Name = "Test Card",
                Hp = 10,
                Attack = 5,
                Cost = 3,
                Description = "A test card",
                Picture = "test.png",
                Effect_active = 0,
                CardTypeId = 1,
                RarityId = 1,
                ExtensionId = 1
            });
            await db.SaveChangesAsync();

            var controller = new CardsController(db);
            var actionResult = await controller.deleteCard(5);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
            var payload = notFoundResult.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Carte introuvable", payload);

            var existingCard = await db.Cards.FindAsync(1);
            Assert.NotNull(existingCard);
            Assert.Equal("Test Card", existingCard.Name);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.SearchCards"/> retourne les cartes correspondant à un nom donné.
        /// </summary>
        [Fact]
        public async Task searchCards_byName_returnsMatchingCards()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            db.Cards.AddRange(
                new Card
                {
                    Id = 1,
                    Name = "Dragon",
                    Hp = 50,
                    Attack = 30,
                    Cost = 10,
                    Description = "A fierce dragon",
                    Picture = "dragon.png",
                    Effect_active = 1,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                },
                new Card
                {
                    Id = 2,
                    Name = "Knight",
                    Hp = 40,
                    Attack = 20,
                    Cost = 8,
                    Description = "A brave knight",
                    Picture = "knight.png",
                    Effect_active = 2,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                },
                new Card
                {
                    Id = 3,
                    Name = "Dragon Rider",
                    Hp = 45,
                    Attack = 25,
                    Cost = 9,
                    Description = "Rides a dragon",
                    Picture = "dragonrider.png",
                    Effect_active = 3,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                });
            await db.SaveChangesAsync();

            var controller = new CardsController(db);
            var actionResult = await controller.searchCards(name: "Dragon", card_type_id: null, rarity_id: null);

            var ok_result = Assert.IsType<OkObjectResult>(actionResult.Result);
            var cards = Assert.IsAssignableFrom<IEnumerable<CardDTO>>(ok_result.Value);

            Assert.NotNull(cards);
            Assert.Equal(2, cards.Count());
            Assert.All(cards, c => Assert.Contains("Dragon", c.name));
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.SearchCards"/> retourne les cartes correspondant à plusieurs paramètres de recherche.
        /// </summary>
        [Fact]
        public async Task searchCards_withMultipleParameters_returnsFilteredCards()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.AddRange(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.AddRange(new Rarity { Id = 1, Label = "Common" }, new Rarity { Id = 2, Label = "Rare" });
            db.Extensions.AddRange(new Extension { Id = 1, Label = "Base Set" });
            db.Cards.AddRange(
                new Card
                {
                    Id = 1,
                    Name = "Dragon Rider",
                    Hp = 50,
                    Attack = 30,
                    Cost = 10,
                    Description = "A fierce dragon",
                    Picture = "dragon.png",
                    Effect_active = 1,
                    CardTypeId = 1,
                    RarityId = 2,
                    ExtensionId = 1
                },
                new Card
                {
                    Id = 2,
                    Name = "Dragon",
                    Hp = 40,
                    Attack = 20,
                    Cost = 8,
                    Description = "A brave knight",
                    Picture = "knight.png",
                    Effect_active = 2,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                });
            await db.SaveChangesAsync();

            var controller = new CardsController(db);
            var actionResult = await controller.searchCards(name: "Dragon", card_type_id: 1, rarity_id: 2);

            var ok_result = Assert.IsType<OkObjectResult>(actionResult.Result);
            var cards = Assert.IsAssignableFrom<IEnumerable<CardDTO>>(ok_result.Value);

            Assert.NotNull(cards);
            var cardList = cards.ToList();
            Assert.Single(cardList);
            Assert.Equal("Dragon Rider", cardList.First().name);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.SearchCards"/> retourne une liste vide lorsqu'aucune carte ne correspond.
        /// </summary>
        [Fact]
        public async Task searchCards_withNoMatches_returnsEmptyList()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            db.Cards.AddRange(
                new Card
                {
                    Id = 1,
                    Name = "Dragon",
                    Hp = 50,
                    Attack = 30,
                    Cost = 10,
                    Description = "A fierce dragon",
                    Picture = "dragon.png",
                    Effect_active = 1,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                },
                new Card
                {
                    Id = 2,
                    Name = "Knight",
                    Hp = 40,
                    Attack = 20,
                    Cost = 8,
                    Description = "A brave knight",
                    Picture = "knight.png",
                    Effect_active = 2,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                });
            await db.SaveChangesAsync();

            var controller = new CardsController(db);
            var actionResult = await controller.searchCards(name: "Wizard", card_type_id: null, rarity_id: null);

            var ok_result = Assert.IsType<OkObjectResult>(actionResult.Result);
            var cards = Assert.IsAssignableFrom<IEnumerable<CardDTO>>(ok_result.Value);

            Assert.NotNull(cards);
            Assert.Empty(cards);
        }

        /// <summary>
        /// Teste si la méthode <see cref="CardsController.SearchCards"/> retourne toutes les cartes lorsqu'aucun paramètre n'est fourni.
        /// </summary>
        [Fact]
        public async Task searchCards_withNoParameters_returnsAllCards()
        {
            var db = get_in_memory_db_context();

            db.CardTypes.Add(new CardType { Id = 1, Label = "Creature" });
            db.Rarities.Add(new Rarity { Id = 1, Label = "Common" });
            db.Extensions.Add(new Extension { Id = 1, Label = "Base Set" });
            db.Cards.AddRange(
                new Card
                {
                    Id = 1,
                    Name = "Dragon",
                    Hp = 50,
                    Attack = 30,
                    Cost = 10,
                    Description = "A fierce dragon",
                    Picture = "dragon.png",
                    Effect_active = 1,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                },
                new Card
                {
                    Id = 2,
                    Name = "Knight",
                    Hp = 40,
                    Attack = 20,
                    Cost = 8,
                    Description = "A brave knight",
                    Picture = "knight.png",
                    Effect_active = 2,
                    CardTypeId = 1,
                    RarityId = 1,
                    ExtensionId = 1
                });
            await db.SaveChangesAsync();

            var controller = new CardsController(db);
            var actionResult = await controller.searchCards(name: null, card_type_id: null, rarity_id: null);

            var ok_result = Assert.IsType<OkObjectResult>(actionResult.Result);
            var cards = Assert.IsAssignableFrom<IEnumerable<CardDTO>>(ok_result.Value);

            Assert.NotNull(cards);
            Assert.Equal(2, cards.Count());
        }
    }
}