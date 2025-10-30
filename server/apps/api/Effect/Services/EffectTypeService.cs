 using api.Effect.DTOs;
 using api.Effect.Providers;
 using VortexTCG.Common.DTO;
using VortexTCG.DataAccess.Models;


namespace api.Effect.Services
{
    public class EffectTypeService
    {
        private readonly EffectTypeProvider _provider;

        public EffectTypeService(EffectTypeProvider provider)   
        {
            _provider = provider;
        }

        private static string Normalize(string s) => s?.Trim() ?? string.Empty;

        private static EffectTypeDTO Map(EffectType e) => new()
        {
            Id = e.Id,
            Label = e.Label
        };

        public async Task<ResultDTO<List<EffectTypeDTO>>> listAsync(CancellationToken ct = default)
        {
            var entities = await _provider.listAsync(ct);
            return new ResultDTO<List<EffectTypeDTO>>
            {
                success = true,
                statusCode = 200,
                data = entities.Select(Map).ToList()
            };
        }

        public async Task<ResultDTO<EffectTypeDTO>> getAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _provider.findByIdAsync(id, ct);
            if (entity is null)
                return new ResultDTO<EffectTypeDTO> { success = false, statusCode = 404, message = "EffectType introuvable." };

            return new ResultDTO<EffectTypeDTO> { success = true, statusCode = 200, data = Map(entity) };
        }

        public async Task<ResultDTO<EffectTypeDTO>> createAsync(EffectTypeCreateDTO input, CancellationToken ct = default)
        {
            var label = Normalize(input.Label);
            if (string.IsNullOrWhiteSpace(label))
                return new ResultDTO<EffectTypeDTO> { success = false, statusCode = 400, message = "Label requis." };

            if (await _provider.existsByLabelAsync(label, ct))
                return new ResultDTO<EffectTypeDTO> { success = false, statusCode = 409, message = "Un EffectType avec ce label existe déjà." };

            var entity = new EffectType
            {
                Id = Guid.NewGuid(),
                Label = label
            };

            entity = await _provider.addAsync(entity, ct);
            return new ResultDTO<EffectTypeDTO> { success = true, statusCode = 201, data = Map(entity) };
        }

        public async Task<ResultDTO<EffectTypeDTO>> updateAsync(Guid id, EffectTypeUpdateDTO input, CancellationToken ct = default)
        {
            var existing = await _provider.findByIdAsync(id, ct);
            if (existing is null)
                return new ResultDTO<EffectTypeDTO> { success = false, statusCode = 404, message = "EffectType introuvable." };

            var label = Normalize(input.Label);
            if (string.IsNullOrWhiteSpace(label))
                return new ResultDTO<EffectTypeDTO> { success = false, statusCode = 400, message = "Label requis." };

            // Unicité simple : autorise le même label si c'est le même enregistrement
            if (!string.Equals(existing.Label, label, StringComparison.Ordinal) &&
                await _provider.existsByLabelAsync(label, ct))
            {
                return new ResultDTO<EffectTypeDTO> { success = false, statusCode = 409, message = "Un EffectType avec ce label existe déjà." };
            }

            existing.Label = label;

            await _provider.updateAsync(existing, ct);
            return new ResultDTO<EffectTypeDTO> { success = true, statusCode = 200, data = Map(existing) };
        }

        public async Task<ResultDTO<bool>> deleteAsync(Guid id, CancellationToken ct = default)
        {
            var ok = await _provider.deleteAsync(id, ct);
            if (!ok)
                return new ResultDTO<bool> { success = false, statusCode = 404, message = "EffectType introuvable." };

            return new ResultDTO<bool> { success = true, statusCode = 200, data = true };
        }
    }
}
