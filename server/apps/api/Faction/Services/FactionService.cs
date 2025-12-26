using VortexTCG.DataAccess.Models;
using VortexTCG.Faction.DTOs;
using VortexTCG.Faction.Providers;
using VortexTCG.Common.DTO;
using DataFaction = VortexTCG.DataAccess.Models.Faction;


namespace VortexTCG.Faction.Services {

    public class FactionService
    {

        private readonly FactionProvider _provider;

        public FactionService(FactionProvider provider)
        {
            _provider = provider;
        }

        private static FactionDto MapFaction(DataFaction entity)
        {
            FactionDto dto = new FactionDto
            {
                Id = entity.Id,
                Label = entity.Label,
                Currency = entity.Currency,
                Condition = entity.Condition
            };
            return dto;
        }

        private static FactionWithCardsDto MapFactionWithCards(DataFaction entity)
        {
            FactionWithCardsDto dto = new FactionWithCardsDto
            {
                Id = entity.Id,
                Label = entity.Label,
                Currency = entity.Currency,
                Condition = entity.Condition,
                Cards = entity.Cards
                    .Select(fc => new FactionCardDto
                    {
                        Id = fc.Card.Id,
                        Name = fc.Card.Name,
                        Price = fc.Card.Price,
                        Hp = fc.Card.Hp,
                        Attack = fc.Card.Attack,
                        Cost = fc.Card.Cost,
                        Description = fc.Card.Description,
                        Picture = fc.Card.Picture,
                        Extension = fc.Card.Extension.ToString(),
                        CardType = fc.Card.CardType.ToString()
                    })
                    .ToList()
            };
            return dto;
        }

        private static FactionWithChampionDto MapFactionWithChampion(DataFaction entity)
        {
            FactionWithChampionDto dto = new FactionWithChampionDto
            {
                Id = entity.Id,
                Label = entity.Label,
                Currency = entity.Currency,
                Condition = entity.Condition,
                Champion = entity.Champions
                    .Select(champion => new FactionChampionDto
                    {
                        Id = champion.Id,
                        Name = champion.Name,
                        Description = champion.Description,
                        HP = champion.HP,
                        Picture = champion.Picture
                    })
                    .FirstOrDefault()
            };
            return dto;
        }
        public async Task<ResultDTO<List<FactionDto>>> GetAllFactions()
        {
            try
            {
                List<DataFaction> factions = await _provider.GetAllAsync();
                return new ResultDTO<List<FactionDto>>
                {
                    statusCode = 200,
                    success = true,
                    message = "Factions retrieved successfully",
                    data = factions.Select(MapFaction).ToList()
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<List<FactionDto>>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error retrieving factions: {ex.Message}",
                    data = new List<FactionDto>()
                };
            }
        }

        public async Task<ResultDTO<FactionDto>> GetFactionById(Guid id)
        {
            try
            {
                DataFaction? faction = await _provider.GetByIdAsync(id);
                if (faction == null)
                {
                    return new ResultDTO<FactionDto>
                    {
                        statusCode = 404,
                        success = false,
                        message = "Faction not found",
                        data = null
                    };
                }

                return new ResultDTO<FactionDto>
                {
                    statusCode = 200,
                    success = true,
                    message = "Faction retrieved successfully",
                    data = MapFaction(faction)
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<FactionDto>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error retrieving faction: {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<ResultDTO<FactionWithCardsDto>> GetFactionWithCardsById(Guid id)
        {
            try
            {
                DataFaction? faction = await _provider.GetWithCardsAsync(id);
                if (faction == null)
                {
                    return new ResultDTO<FactionWithCardsDto>
                    {
                        statusCode = 404,
                        success = false,
                        message = "Faction not found",
                        data = null
                    };
                }

                return new ResultDTO<FactionWithCardsDto>
                {
                    statusCode = 200,
                    success = true,
                    message = "Faction with cards retrieved successfully",
                    data = MapFactionWithCards(faction)
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<FactionWithCardsDto>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error retrieving faction with cards: {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<ResultDTO<FactionWithChampionDto>> GetFactionWithChampionById(Guid id)
        {
            try
            {
                DataFaction? faction = await _provider.GetWithChampionAsync(id);
                if (faction == null)
                {
                    return new ResultDTO<FactionWithChampionDto>
                    {
                        statusCode = 404,
                        success = false,
                        message = "Faction not found",
                        data = null
                    };
                }

                return new ResultDTO<FactionWithChampionDto>
                {
                    statusCode = 200,
                    success = true,
                    message = "Faction with champion retrieved successfully",
                    data = MapFactionWithChampion(faction)
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<FactionWithChampionDto>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error retrieving faction with champions: {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<ResultDTO<FactionDto>> CreateFaction(CreateFactionDto createDto)
        {
            try
            {
                if (await _provider.LabelExists(createDto.Label))
                {
                    return new ResultDTO<FactionDto>
                    {
                        statusCode = 409,
                        success = false,
                        message = "A faction with this label already exists",
                        data = null
                    };
                }

                (bool IsValid, List<Guid> InvalidIds) cardCheck = await _provider.ValidateCardIds(createDto.CardIds);
                if (!cardCheck.IsValid)
                {
                    string errorMessageCards = string.Join(", ", cardCheck.InvalidIds);
                    return new ResultDTO<FactionDto>
                    {
                        statusCode = 400,
                        success = false,
                        message = $"Les IDs de cartes suivants n'existent pas : {errorMessageCards}",
                        data = null
                    };
                }

                if (createDto.ChampionId.HasValue)
                {
                    bool championValid = await _provider.ValidateChampionId(createDto.ChampionId.Value);
                    if (!championValid)
                    {
                        return new ResultDTO<FactionDto>
                        {
                            statusCode = 400,
                            success = false,
                            message = $"L'ID du champion {createDto.ChampionId.Value} n'existe pas",
                            data = null
                        };
                    }
                }

                DataFaction factionEntity = new DataFaction
                {
                    Id = Guid.NewGuid(),
                    Label = createDto.Label,
                    Currency = createDto.Currency,
                    Condition = createDto.Condition,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedAtUtc = DateTime.UtcNow,
                    UpdatedBy = "System"
                };

                factionEntity = await _provider.AddAsync(factionEntity);

                if (createDto.CardIds.Any())
                {
                    await _provider.ReplaceCardsAsync(factionEntity.Id, createDto.CardIds);
                }

                if (createDto.ChampionId.HasValue)
                {
                    await _provider.SetChampionAsync(factionEntity.Id, createDto.ChampionId.Value);
                }

                FactionDto resultDto = MapFaction(factionEntity);

                return new ResultDTO<FactionDto>
                {
                    statusCode = 201,
                    success = true,
                    message = "Faction created successfully",
                    data = resultDto
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<FactionDto>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error creating faction: {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<ResultDTO<FactionDto>> UpdateFaction(Guid id, UpdateFactionDto updateDto)
        {
            try
            {
                if (!await _provider.FactionExists(id))
                {
                    return new ResultDTO<FactionDto>
                    {
                        statusCode = 404,
                        success = false,
                        message = "Faction not found",
                        data = null
                    };
                }

                if (!string.IsNullOrEmpty(updateDto.Label) && await _provider.LabelExists(updateDto.Label, id))
                {
                    return new ResultDTO<FactionDto>
                    {
                        statusCode = 409,
                        success = false,
                        message = "A faction with this label already exists",
                        data = null
                    };
                }

                // Validation des IDs
                if (updateDto.CardIds != null)
                {
                    (bool IsValid, List<Guid> InvalidIds) cardCheck = await _provider.ValidateCardIds(updateDto.CardIds);
                    if (!cardCheck.IsValid)
                    {
                        string errorMessageCards = string.Join(", ", cardCheck.InvalidIds);
                        return new ResultDTO<FactionDto>
                        {
                            statusCode = 400,
                            success = false,
                            message = $"Les IDs de cartes suivants n'existent pas : {errorMessageCards}",
                            data = null
                        };
                    }
                }

                if (updateDto.ChampionId.HasValue)
                {
                    bool championValid = await _provider.ValidateChampionId(updateDto.ChampionId.Value);
                    if (!championValid)
                    {
                        return new ResultDTO<FactionDto>
                        {
                            statusCode = 400,
                            success = false,
                            message = $"L'ID du champion {updateDto.ChampionId.Value} n'existe pas",
                            data = null
                        };
                    }
                }

                DataFaction? factionEntity = await _provider.GetByIdTrackedAsync(id);
                if (factionEntity == null)
                {
                    return new ResultDTO<FactionDto>
                    {
                        statusCode = 404,
                        success = false,
                        message = "Faction not found",
                        data = null
                    };
                }

                if (!string.IsNullOrEmpty(updateDto.Label))
                    factionEntity.Label = updateDto.Label;

                if (!string.IsNullOrEmpty(updateDto.Currency))
                    factionEntity.Currency = updateDto.Currency;

                if (updateDto.Condition != null)
                    factionEntity.Condition = updateDto.Condition;

                if (updateDto.CardIds != null)
                {
                    await _provider.ReplaceCardsAsync(factionEntity.Id, updateDto.CardIds);
                }

                if (updateDto.ChampionId.HasValue)
                {
                    await _provider.SetChampionAsync(factionEntity.Id, updateDto.ChampionId.Value);
                }

                factionEntity.UpdatedAtUtc = DateTime.UtcNow;
                factionEntity.UpdatedBy = "System";

                await _provider.UpdateAsync(factionEntity);

                FactionDto resultDto = MapFaction(factionEntity);

                return new ResultDTO<FactionDto>
                {
                    statusCode = 200,
                    success = true,
                    message = "Faction updated successfully",
                    data = resultDto
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<FactionDto>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error updating faction: {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<ResultDTO<object>> DeleteFaction(Guid id)
        {
            try
            {
                var success = await _provider.DeleteFaction(id);
                if (!success)
                {
                    return new ResultDTO<object>
                    {
                        statusCode = 404,
                        success = false,
                        message = "Faction not found",
                        data = null
                    };
                }

                return new ResultDTO<object>
                {
                    statusCode = 200,
                    success = true,
                    message = "Faction deleted successfully",
                    data = null
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<object>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error deleting faction: {ex.Message}",
                    data = null
                };
            }
        }
    }
}