using System.Security.Claims;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Faction.DTOs;
using VortexTCG.Faction.Providers;
using VortexTCG.Common.DTO;
using System.Text;


namespace VortexTCG.Faction.Services {

    public class FactionService
    {

        private readonly VortexDbContext _db;
        private readonly IConfiguration _configuration;

        private readonly FactionProvider _provider;

        public FactionService(VortexDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _provider = new FactionProvider(_db);
        }
        public async Task<ResultDTO<List<FactionDTO>>> GetAllFactions()
        {
            try
            {
                var factions = await _provider.GetAllFactions();
                return new ResultDTO<List<FactionDTO>>
                {
                    statusCode = 200,
                    success = true,
                    message = "Factions retrieved successfully",
                    data = factions
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<List<FactionDTO>>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error retrieving factions: {ex.Message}",
                    data = new List<FactionDTO>()
                };
            }
        }

        public async Task<ResultDTO<FactionDTO>> GetFactionById(Guid id)
        {
            try
            {
                var faction = await _provider.GetFactionById(id);
                if (faction == null)
                {
                    return new ResultDTO<FactionDTO>
                    {
                        statusCode = 404,
                        success = false,
                        message = "Faction not found",
                        data = null
                    };
                }

                return new ResultDTO<FactionDTO>
                {
                    statusCode = 200,
                    success = true,
                    message = "Faction retrieved successfully",
                    data = faction
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<FactionDTO>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error retrieving faction: {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<ResultDTO<FactionWithCardsDTO>> GetFactionWithCardsById(Guid id)
        {
            try
            {
                var faction = await _provider.GetFactionWithCardsById(id);
                if (faction == null)
                {
                    return new ResultDTO<FactionWithCardsDTO>
                    {
                        statusCode = 404,
                        success = false,
                        message = "Faction not found",
                        data = null
                    };
                }

                return new ResultDTO<FactionWithCardsDTO>
                {
                    statusCode = 200,
                    success = true,
                    message = "Faction with cards retrieved successfully",
                    data = faction
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<FactionWithCardsDTO>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error retrieving faction with cards: {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<ResultDTO<FactionWithChampionDTO>> GetFactionWithChampionById(Guid id)
        {
            try
            {
                var faction = await _provider.GetFactionWithChampionById(id);
                if (faction == null)
                {
                    return new ResultDTO<FactionWithChampionDTO>
                    {
                        statusCode = 404,
                        success = false,
                        message = "Faction not found",
                        data = null
                    };
                }

                return new ResultDTO<FactionWithChampionDTO>
                {
                    statusCode = 200,
                    success = true,
                    message = "Faction with champion retrieved successfully",
                    data = faction
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<FactionWithChampionDTO>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error retrieving faction with champions: {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<ResultDTO<FactionDTO>> CreateFaction(CreateFactionDTO createDto)
        {
            try
            {
                if (await _provider.LabelExists(createDto.Label))
                {
                    return new ResultDTO<FactionDTO>
                    {
                        statusCode = 409,
                        success = false,
                        message = "A faction with this label already exists",
                        data = null
                    };
                }

                // ✅ NOUVEAU : Gestion de la validation des IDs
                var (success, faction, errorMessage) = await _provider.CreateFaction(createDto);
                
                if (!success)
                {
                    return new ResultDTO<FactionDTO>
                    {
                        statusCode = 400, // Bad Request pour les IDs invalides
                        success = false,
                        message = errorMessage,
                        data = null
                    };
                }

                return new ResultDTO<FactionDTO>
                {
                    statusCode = 201,
                    success = true,
                    message = "Faction created successfully",
                    data = faction!
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<FactionDTO>
                {
                    statusCode = 500,
                    success = false,
                    message = $"Error creating faction: {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<ResultDTO<FactionDTO>> UpdateFaction(Guid id, UpdateFactionDTO updateDto)
        {
            try
            {
                if (!await _provider.FactionExists(id))
                {
                    return new ResultDTO<FactionDTO>
                    {
                        statusCode = 404,
                        success = false,
                        message = "Faction not found",
                        data = null
                    };
                }

                if (!string.IsNullOrEmpty(updateDto.Label) && await _provider.LabelExists(updateDto.Label, id))
                {
                    return new ResultDTO<FactionDTO>
                    {
                        statusCode = 409,
                        success = false,
                        message = "A faction with this label already exists",
                        data = null
                    };
                }

                // ✅ NOUVEAU : Gestion de la validation des IDs
                var (success, faction, errorMessage) = await _provider.UpdateFaction(id, updateDto);
                
                if (!success)
                {
                    return new ResultDTO<FactionDTO>
                    {
                        statusCode = 400, // Bad Request pour les IDs invalides
                        success = false,
                        message = errorMessage,
                        data = null
                    };
                }

                return new ResultDTO<FactionDTO>
                {
                    statusCode = 200,
                    success = true,
                    message = "Faction updated successfully",
                    data = faction!
                };
            }
            catch (Exception ex)
            {
                return new ResultDTO<FactionDTO>
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