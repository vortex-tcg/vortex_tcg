using VortexTCG.Api.Rank.DTOs;
using VortexTCG.Api.Rank.Providers;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess.Models;
using RankModel = VortexTCG.DataAccess.Models.Rank;

namespace VortexTCG.Api.Rank.Services
{
    public class RankService
    {
        private readonly RankProvider _provider;
        public RankService(RankProvider provider)
        {
            _provider = provider;
        }

        private static RankDTO Map(RankModel e) => new()
        {
            Id = e.Id,
            Label = e.Label,
            nbVictory = e.nbVictory
        };

        public async Task<ResultDTO<RankDTO>> CreateAsync(RankCreateDTO input, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(input.Label))
                return new ResultDTO<RankDTO> { success = false, statusCode = 400, message = "Label requis" };

            var exists = await _provider.GetAllAsync();
            if (exists.Exists(r => r.Label == input.Label))
                return new ResultDTO<RankDTO> { success = false, statusCode = 409, message = "Un rank avec ce label existe déjà" };

            var entity = new RankModel
            {
                Id = Guid.NewGuid(),
                Label = input.Label,
                nbVictory = input.nbVictory
            };

            entity = await _provider.AddAsync(entity);
            return new ResultDTO<RankDTO> { success = true, statusCode = 201, message = "Rank créé avec succès", data = Map(entity) };
        }

        public async Task<ResultDTO<RankDTO>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var rank = await _provider.GetByIdAsync(id);
            if (rank == null)
                return new ResultDTO<RankDTO> { success = false, statusCode = 404, message = "Rank non trouvé" };
            return new ResultDTO<RankDTO> { success = true, statusCode = 200, data = Map(rank) };
        }

        public async Task<ResultDTO<RankDTO[]>> GetAllAsync(CancellationToken ct = default)
        {
            var ranks = await _provider.GetAllAsync();
            var dtos = ranks.ConvertAll(Map).ToArray();
            return new ResultDTO<RankDTO[]> { success = true, statusCode = 200, data = dtos };
        }

        public async Task<ResultDTO<RankDTO>> UpdateAsync(Guid id, RankCreateDTO input, CancellationToken ct = default)
        {
            var rank = await _provider.GetByIdAsync(id);
            if (rank == null)
                return new ResultDTO<RankDTO> { success = false, statusCode = 404, message = "Rank non trouvé" };

            rank.Label = input.Label;
            rank.nbVictory = input.nbVictory;
            var success = await _provider.UpdateAsync(rank);
            if (!success)
                return new ResultDTO<RankDTO> { success = false, statusCode = 500, message = "Erreur lors de la mise à jour" };
            return new ResultDTO<RankDTO> { success = true, statusCode = 200, message = "Rank mis à jour", data = Map(rank) };
        }

        public async Task<ResultDTO<object>> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var success = await _provider.DeleteAsync(id);
            if (!success)
                return new ResultDTO<object> { success = false, statusCode = 404, message = "Rank non trouvé" };
            return new ResultDTO<object> { success = true, statusCode = 204, message = "Rank supprimé" };
        }
    }
}