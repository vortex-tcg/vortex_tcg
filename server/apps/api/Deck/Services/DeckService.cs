using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Providers;
using VortexTCG.Common.DTO;
using DeckModel = VortexTCG.DataAccess.Models.Deck;

namespace VortexTCG.Api.Deck.Services
{
    public class DeckService
    {
        private readonly DeckProvider _provider;

        private static DeckDto Map(DeckModel entity) => new DeckDto
        {
            Id = entity.Id,
            Label = entity.Label,
            UserId = entity.UserId,
            ChampionId = entity.ChampionId,
            FactionId = entity.FactionId
        };

        public DeckService(DeckProvider provider)
        {
            _provider = provider;
        }

        public async Task<ResultDTO<DeckDto[]>> GetAllAsync(CancellationToken ct = default)
        {
            List<DeckModel> entities = await _provider.GetAllAsync(ct);
            ResultDTO<DeckDto[]> result = new ResultDTO<DeckDto[]>
            {
                success = true,
                statusCode = 200,
                data = entities.Select(Map).ToArray()
            };
            return result;
        }

        public async Task<ResultDTO<DeckDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            DeckModel? entity = await _provider.GetByIdAsync(id, ct);
            if (entity is null)
            {
                ResultDTO<DeckDto> notFound = new ResultDTO<DeckDto>
                {
                    success = false,
                    statusCode = 404,
                    message = "Deck non trouvé"
                };
                return notFound;
            }
            ResultDTO<DeckDto> ok = new ResultDTO<DeckDto>
            {
                success = true,
                statusCode = 200,
                data = Map(entity)
            };
            return ok;
        }

        public async Task<ResultDTO<DeckDto[]>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            List<DeckModel> entities = await _provider.GetByUserIdAsync(userId, ct);
            ResultDTO<DeckDto[]> result = new ResultDTO<DeckDto[]>
            {
                success = true,
                statusCode = 200,
                data = entities.Select(Map).ToArray()
            };
            return result;
        }

        public async Task<ResultDTO<DeckDto>> CreateAsync(DeckCreateDto input, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(input.Label))
                return new ResultDTO<DeckDto> { success = false, statusCode = 400, message = "Label requis" };

            if (await _provider.ExistsByLabelAndUserAsync(input.Label, input.UserId, ct))
                return new ResultDTO<DeckDto> { success = false, statusCode = 409, message = "Un deck avec ce nom existe déjà pour cet utilisateur" };

            DeckModel entity = new DeckModel
            {
                Id = Guid.NewGuid(),
                Label = input.Label,
                UserId = input.UserId,
                ChampionId = input.ChampionId,
                FactionId = input.FactionId
            };

            entity = await _provider.AddAsync(entity, ct);
            ResultDTO<DeckDto> created = new ResultDTO<DeckDto>
            {
                success = true,
                statusCode = 201,
                message = "Deck créé avec succès",
                data = Map(entity)
            };
            return created;
        }

        public async Task<ResultDTO<DeckDto>> UpdateAsync(Guid id, DeckUpdateDto input, CancellationToken ct = default)
        {
            DeckModel? entity = await _provider.GetByIdTrackedAsync(id, ct);
            if (entity is null)
            {
                ResultDTO<DeckDto> notFound = new ResultDTO<DeckDto>
                {
                    success = false,
                    statusCode = 404,
                    message = "Deck non trouvé"
                };
                return notFound;
            }

            if (string.IsNullOrWhiteSpace(input.Label))
                return new ResultDTO<DeckDto> { success = false, statusCode = 400, message = "Label requis" };

            entity.Label = input.Label;
            entity.ChampionId = input.ChampionId;
            entity.FactionId = input.FactionId;

            entity = await _provider.UpdateAsync(entity, ct);
            ResultDTO<DeckDto> updated = new ResultDTO<DeckDto>
            {
                success = true,
                statusCode = 200,
                message = "Deck mis à jour avec succès",
                data = Map(entity)
            };
            return updated;
        }

        public async Task<ResultDTO<object>> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            DeckModel? entity = await _provider.GetByIdTrackedAsync(id, ct);
            if (entity is null)
                return new ResultDTO<object> { success = false, statusCode = 404, message = "Deck non trouvé", data = null };

            await _provider.DeleteAsync(entity, ct);
            return new ResultDTO<object> { success = true, statusCode = 200, message = "Deck supprimé avec succès", data = null };
        }
    }
}
