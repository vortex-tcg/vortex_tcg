using VortexTCG.Api.Card.DTOs;
using VortexTCG.Api.Collection.DTOs;
using VortexTCG.Api.Collection.Providers;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess.Models;
using CollectionModel = VortexTCG.DataAccess.Models.Collection;
using DeckModel = VortexTCG.DataAccess.Models.Deck;

namespace VortexTCG.Api.Collection.Services
{
    public class CollectionService
    {
        private readonly CollectionProvider _provider;
        private readonly VortexTCG.Api.Deck.Providers.DeckProvider _deckProvider;

        public CollectionService(CollectionProvider provider, VortexTCG.Api.Deck.Providers.DeckProvider deckProvider)
        {
            _provider = provider;
            _deckProvider = deckProvider;
        }

        private static CollectionDto Map(CollectionModel e) => new()
        {
            Id = e.Id
        };

        public async Task<ResultDTO<CollectionDto>> CreateAsync(CollectionCreateDto input, CancellationToken ct = default)
        {
            if (input.UserId == Guid.Empty)
            {
                return new ResultDTO<CollectionDto>
                {
                    success = false,
                    statusCode = 400,
                    message = "UserId requis"
                };
            }

            CollectionModel entity = new CollectionModel
            {
                Id = Guid.NewGuid(),
                User = new VortexTCG.DataAccess.Models.User { Id = input.UserId },
                Cards = new List<CollectionCard>(),
                Champions = new List<CollectionChampion>()
            };

            entity = await _provider.AddAsync(entity);

            return new ResultDTO<CollectionDto>
            {
                success = true,
                statusCode = 201,
                message = "Collection créée avec succès",
                data = Map(entity)
            };
        }

        public async Task<ResultDTO<CollectionDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            CollectionModel? collection = await _provider.GetByIdAsync(id);

            if (collection == null)
            {
                return new ResultDTO<CollectionDto>
                {
                    success = false,
                    statusCode = 404,
                    message = "Collection non trouvée"
                };
            }

            return new ResultDTO<CollectionDto>
            {
                success = true,
                statusCode = 200,
                data = Map(collection)
            };
        }

        public async Task<ResultDTO<UserCollectionDto>> GetCollectionByUserId(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
            {
                return new ResultDTO<UserCollectionDto>
                {
                    success = false,
                    statusCode = 400,
                    message = "UserId requis"
                };
            }

            CollectionModel? collection = await _provider.GetByUserIdAsync(id);

            if (collection == null)
            {
                return new ResultDTO<UserCollectionDto>
                {
                    success = false,
                    statusCode = 404,
                    message = "Collection utilisateur introuvable"
                };
            }

            List<UserCollectionCardDto> cards = collection.Cards?
                .Where(cc => cc.Card != null)
                .Select(cc => new UserCollectionCardDto
                {
                    CollectionCardId = cc.Id,
                    Card = new CardDto
                    {
                        Id = cc.Card.Id,
                        Name = cc.Card.Name,
                        Price = cc.Card.Price,
                        Description = cc.Card.Description,
                        Hp = cc.Card.Hp ?? 0,
                        Attack = cc.Card.Attack ?? 0,
                        Cost = cc.Card.Cost,
                        Extension = cc.Card.Extension.ToString(),
                        CardType = cc.Card.CardType.ToString(),
                        Class = cc.Card.Class?
                            .Where(x => x.Class != null)
                            .Select(x => x.Class.ToString())
                            .ToList() ?? new List<string>(),
                        Factions = cc.Card.Factions?.Select(x => x.FactionId).ToList() ?? new List<Guid>()
                    },
                    OwnData = new List<UserCollectionCardOwnDto>
                    {
                        new UserCollectionCardOwnDto
                        {
                            Number = cc.Quantity,
                            Rarity = cc.Rarity.ToString()
                        }
                    }
                })
                .ToList() ?? new List<UserCollectionCardDto>();

            UserCollectionDto dto = new UserCollectionDto
            {
                Decks = new List<UserCollectionDeckDto>(),
                Faction = new List<UserCollectionFactionDto>(),
                Cards = cards
            };

            // Try to populate user's decks
            try
            {
                List<DeckModel> decks = await _deckProvider.GetDecksByUserIdAsync(id);
                if (decks != null && decks.Count > 0)
                {
                    dto.Decks = decks.Select(d => new UserCollectionDeckDto
                    {
                        DeckId = d.Id,
                        ChampionId = d.ChampionId,
                        FactionId = d.FactionId,
                        DeckName = string.IsNullOrWhiteSpace(d.Label) ? "Deck" : d.Label,
                        ChampionImage = d.Champion?.Picture ?? string.Empty
                    }).ToList();
                }
            }
            catch
            {
                // ignore deck population errors
            }

            return new ResultDTO<UserCollectionDto>
            {
                success = true,
                statusCode = 200,
                data = dto
            };
        }

        public async Task<ResultDTO<CollectionDto[]>> GetAllAsync(CancellationToken ct = default)
        {
            List<CollectionModel> collections = await _provider.GetAllAsync();
            CollectionDto[] dtos = collections.ConvertAll(Map).ToArray();

            return new ResultDTO<CollectionDto[]>
            {
                success = true,
                statusCode = 200,
                data = dtos
            };
        }

        public async Task<ResultDTO<CollectionDto>> UpdateAsync(Guid id, CollectionCreateDto input, CancellationToken ct = default)
        {
            CollectionModel? collection = await _provider.GetByIdAsync(id);

            if (collection == null)
            {
                return new ResultDTO<CollectionDto>
                {
                    success = false,
                    statusCode = 404,
                    message = "Collection non trouvée"
                };
            }

            if (input.UserId == Guid.Empty)
            {
                return new ResultDTO<CollectionDto>
                {
                    success = false,
                    statusCode = 400,
                    message = "UserId requis"
                };
            }

            collection.User = new VortexTCG.DataAccess.Models.User { Id = input.UserId };
            bool success = await _provider.UpdateAsync(collection);

            if (!success)
            {
                return new ResultDTO<CollectionDto>
                {
                    success = false,
                    statusCode = 500,
                    message = "Erreur lors de la mise à jour"
                };
            }

            return new ResultDTO<CollectionDto>
            {
                success = true,
                statusCode = 200,
                message = "Collection mise à jour",
                data = Map(collection)
            };
        }

        public async Task<ResultDTO<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            bool success = await _provider.DeleteAsync(id);

            if (!success)
            {
                return new ResultDTO<bool>
                {
                    success = false,
                    statusCode = 404,
                    message = "Collection non trouvée"
                };
            }

            return new ResultDTO<bool>
            {
                success = true,
                statusCode = 204,
                message = "Collection supprimée",
                data = true
            };
        }
    }
}