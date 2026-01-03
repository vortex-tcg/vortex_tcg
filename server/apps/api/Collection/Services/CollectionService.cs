using VortexTCG.Api.Collection.DTOs;
using VortexTCG.Api.Collection.Providers;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess.Models;
using CollectionModel = VortexTCG.DataAccess.Models.Collection;
using VortexTCG.Api.Card.DTOs;

namespace VortexTCG.Api.Collection.Services
{
    public class CollectionService
    {
        private readonly CollectionProvider _provider;
        public CollectionService(CollectionProvider provider)
        {
            _provider = provider;
        }

        private static CollectionDto Map(CollectionModel e) => new()
        {
            Id = e.Id
        };

        public async Task<ResultDTO<CollectionDto>> CreateAsync(CollectionCreateDto input, CancellationToken ct = default)
        {
            if (input.UserId == Guid.Empty)
                return new ResultDTO<CollectionDto> { success = false, statusCode = 400, message = "UserId requis" };

            CollectionModel entity = new CollectionModel
            {
                Id = Guid.NewGuid()
            };

            entity = await _provider.AddAsync(entity);
            return new ResultDTO<CollectionDto> { success = true, statusCode = 201, message = "Collection créée avec succès", data = Map(entity) };
        }

        public async Task<ResultDTO<CollectionDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            CollectionModel collection = await _provider.GetByIdAsync(id);
            if (collection == null)
                return new ResultDTO<CollectionDto> { success = false, statusCode = 404, message = "Collection non trouvée" };
            return new ResultDTO<CollectionDto> { success = true, statusCode = 200, data = Map(collection) };
        }

        private UserCollectionDto GenUserCollection()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            UserCollectionFactionDto Faction1 = new UserCollectionFactionDto
            {
                FactionId = new Guid(),
                FactionName = "Berzerkouin"
            };
            UserCollectionFactionDto Faction2 = new UserCollectionFactionDto
            {
                FactionId = new Guid(),
                FactionName = "Cacturoquaï"
            };
            UserCollectionDeckDto Deck1 = new UserCollectionDeckDto
            {
                DeckId = new Guid(),
                DeckName = "Emporio PingChilling"
            };
            UserCollectionDeckDto Deck2 = new UserCollectionDeckDto
            {
                DeckId = new Guid(),
                DeckName = "Pot-pourris"
            };

            List<UserCollectionCardDto> listCard = new List<UserCollectionCardDto>();

            for (int i = 0; i != 60; i += 1) {
                if (i % 2 == 1) {
                    listCard.Add(new UserCollectionCardDto{
                        Card = new CardDto {
                            Id = new Guid(),
                            Name = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray()),
                            Price = 0,
                            Description = new string(Enumerable.Repeat(chars, 50).Select(s => s[random.Next(s.Length)]).ToArray()),
                            Hp = random.Next(10),
                            Attack = random.Next(10),
                            Cost = random.Next(10),
                            Extension = "NORMAL",
                            CardType = "GUARD",
                            Class = ["guerrier"],
                            Factions = [Faction1.FactionId]
                        },
                        OwnData = new List<UserCollectionCardOwnDto>([
                            new UserCollectionCardOwnDto {
                                Number = 3,
                                Rarity = "NORMAL"
                            }
                        ])
                    });
                } else {
                    listCard.Add(new UserCollectionCardDto{
                        Card = new CardDto {
                            Id = new Guid(),
                            Name = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray()),
                            Price = 0,
                            Description = new string(Enumerable.Repeat(chars, 50).Select(s => s[random.Next(s.Length)]).ToArray()),
                            Hp = random.Next(10),
                            Attack = random.Next(10),
                            Cost = random.Next(10),
                            Extension = "NORMAL",
                            CardType = "GUARD",
                            Class = ["guerrier"],
                            Factions = [Faction2.FactionId]
                        },
                        OwnData = new List<UserCollectionCardOwnDto>([
                            new UserCollectionCardOwnDto {
                                Number = 3,
                                Rarity = "NORMAL"
                            }
                        ])
                    });
                }
            }

            for (int i = 0; i != 60; i += 1) {
                if (i % 2 == 1) {
                    listCard.Add(new UserCollectionCardDto{
                        Card = new CardDto {
                            Id = new Guid(),
                            Name = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray()),
                            Price = 0,
                            Description = new string(Enumerable.Repeat(chars, 50).Select(s => s[random.Next(s.Length)]).ToArray()),
                            Hp = random.Next(10),
                            Attack = random.Next(10),
                            Cost = random.Next(10),
                            Extension = "NORMAL",
                            CardType = "GUARD",
                            Class = ["guerrier"],
                            Factions = [Faction1.FactionId]
                        }
                    });
                } else {
                    listCard.Add(new UserCollectionCardDto{
                        Card = new CardDto {
                            Id = new Guid(),
                            Name = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray()),
                            Price = 0,
                            Description = new string(Enumerable.Repeat(chars, 50).Select(s => s[random.Next(s.Length)]).ToArray()),
                            Hp = random.Next(10),
                            Attack = random.Next(10),
                            Cost = random.Next(10),
                            Extension = "NORMAL",
                            CardType = "GUARD",
                            Class = ["guerrier"],
                            Factions = [Faction2.FactionId]
                        }
                    });
                }
            }
            return new UserCollectionDto{
                Decks = new List<UserCollectionDeckDto>([Deck1, Deck2]),
                Faction = new List<UserCollectionFactionDto>([Faction1, Faction2]),
                Cards = listCard
            };
        }

        public async Task<ResultDTO<UserCollectionDto>> GetCollectionByUserId(Guid Id, CancellationToken ct = default)
        {
            return new ResultDTO<UserCollectionDto>{
                success = true, statusCode = 200, data = GenUserCollection()
            };
            // if (!checkIfUserExist(Id)) {
                // return new ResultDTO<UserCollectionDto>{
                    // success = false, statusCode = 404, message = "User not found"
                // }
            // }

        }

        public async Task<ResultDTO<CollectionDto[]>> GetAllAsync(CancellationToken ct = default)
        {
            List<CollectionModel> collections = await _provider.GetAllAsync();
            CollectionDto[] dtos = collections.ConvertAll(Map).ToArray();
            return new ResultDTO<CollectionDto[]> { success = true, statusCode = 200, data = dtos };
        }

        public async Task<ResultDTO<CollectionDto>> UpdateAsync(Guid id, CollectionCreateDto input, CancellationToken ct = default)
        {
            CollectionModel collection = await _provider.GetByIdAsync(id);
            if (collection == null)
                return new ResultDTO<CollectionDto> { success = false, statusCode = 404, message = "Collection non trouvée" };

            bool success = await _provider.UpdateAsync(collection);
            if (!success)
                return new ResultDTO<CollectionDto> { success = false, statusCode = 500, message = "Erreur lors de la mise à jour" };
            return new ResultDTO<CollectionDto> { success = true, statusCode = 200, message = "Collection mise à jour", data = Map(collection) };
        }

        public async Task<ResultDTO<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            bool success = await _provider.DeleteAsync(id);
            if (!success)
                return new ResultDTO<bool> { success = false, statusCode = 404, message = "Collection non trouvée" };
            return new ResultDTO<bool> { success = true, statusCode = 204, message = "Collection supprimée", data = true };
        }
    }
}
