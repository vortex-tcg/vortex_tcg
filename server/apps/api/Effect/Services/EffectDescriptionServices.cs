using api.Effect.DTOs;
using api.Effect.Providers;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess.Models;

namespace api.Effect.Services
{
    public class EffectDescriptionService
    {
        private readonly EffectDescriptionProvider _provider;
        public EffectDescriptionService(EffectDescriptionProvider provider) => _provider = provider;

        private static string N(string s) => s?.Trim() ?? string.Empty;

        private static EffectDescriptionDTO Map(EffectDescription e) => new()
        {
            Id = e.Id,
            Label = e.Label,
            Description = e.Description,
            Parameter = e.Parameter
        };

        public async Task<ResultDTO<List<EffectDescriptionDTO>>> listAsync(CancellationToken ct = default)
        {
            var items = await _provider.listAsync(ct);
            return new ResultDTO<List<EffectDescriptionDTO>>
            {
                success = true,
                statusCode = 200,
                data = items.Select(Map).ToList()
            };
        }

        public async Task<ResultDTO<EffectDescriptionDTO>> getAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _provider.findByIdAsync(id, ct);
            if (entity is null)
                return new() { success = false, statusCode = 404, message = "EffectDescription introuvable." };

            return new() { success = true, statusCode = 200, data = Map(entity) };
        }

        public async Task<ResultDTO<EffectDescriptionDTO>> createAsync(EffectDescriptionInputDTO input, CancellationToken ct = default)
        {
            var label = N(input.Label);
            var description = N(input.Description);

            if (string.IsNullOrWhiteSpace(label))
                return new() { success = false, statusCode = 400, message = "Label requis." };
            if (string.IsNullOrWhiteSpace(description))
                return new() { success = false, statusCode = 400, message = "Description requise." };

            if (await _provider.existsByLabelAsync(label, ct))
                return new() { success = false, statusCode = 409, message = "Un EffectDescription avec ce label existe déjà." };

            var entity = await _provider.addAsync(new EffectDescription
            {
                Id = Guid.NewGuid(),
                Label = label,
                Description = description,
                Parameter = string.IsNullOrWhiteSpace(input.Parameter) ? null : input.Parameter!.Trim()
            }, ct);

            return new() { success = true, statusCode = 201, data = Map(entity) };
        }

        public async Task<ResultDTO<EffectDescriptionDTO>> updateAsync(Guid id, EffectDescriptionInputDTO input, CancellationToken ct = default)
        {
            var current = await _provider.findByIdAsync(id, ct);
            if (current is null)
                return new() { success = false, statusCode = 404, message = "EffectDescription introuvable." };

            var label = N(input.Label);
            var description = N(input.Description);

            if (string.IsNullOrWhiteSpace(label))
                return new() { success = false, statusCode = 400, message = "Label requis." };
            if (string.IsNullOrWhiteSpace(description))
                return new() { success = false, statusCode = 400, message = "Description requise." };

            if (!string.Equals(current.Label, label, StringComparison.Ordinal) &&
                await _provider.existsByLabelAsync(label, ct))
            {
                return new() { success = false, statusCode = 409, message = "Un EffectDescription avec ce label existe déjà." };
            }

            current.Label = label;
            current.Description = description;
            current.Parameter = string.IsNullOrWhiteSpace(input.Parameter) ? null : input.Parameter!.Trim();

            await _provider.updateAsync(current, ct);
            return new() { success = true, statusCode = 200, data = Map(current) };
        }

        public async Task<ResultDTO<bool>> deleteAsync(Guid id, CancellationToken ct = default)
        {
            var inUse = await _provider.countEffectsUsingAsync(id, ct);
            if (inUse > 0)
                return new() { success = false, statusCode = 409, message = "Impossible de supprimer : description utilisée par des effets." };

            var ok = await _provider.deleteAsync(id, ct);
            if (!ok) return new() { success = false, statusCode = 404, message = "EffectDescription introuvable." };

            return new() { success = true, statusCode = 200, data = true };
        }
    }
}
