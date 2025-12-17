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

        private static EffectDescriptionDto Map(EffectDescription e) => new()
        {
            Id = e.Id,
            Label = e.Label,
            Description = e.Description,
            Parameter = e.Parameter
        };

        public async Task<ResultDTO<List<EffectDescriptionDto>>> listAsync(CancellationToken ct = default)
        {
            List<EffectDescription> items = await _provider.listAsync(ct);
            return new ResultDTO<List<EffectDescriptionDto>>
            {
                success = true,
                statusCode = 200,
                data = items.Select(Map).ToList()
            };
        }

        public async Task<ResultDTO<EffectDescriptionDto>> getAsync(Guid id, CancellationToken ct = default)
        {
            EffectDescription entity = await _provider.findByIdAsync(id, ct);
            if (entity is null)
                return new() { success = false, statusCode = 404, message = "EffectDescription introuvable." };

            return new() { success = true, statusCode = 200, data = Map(entity) };
        }

        public async Task<ResultDTO<EffectDescriptionDto>> createAsync(EffectDescriptionInputDto input, CancellationToken ct = default)
        {
            string label = N(input.Label);
            string description = N(input.Description);

            if (string.IsNullOrWhiteSpace(label))
                return new() { success = false, statusCode = 400, message = "Label requis." };
            if (string.IsNullOrWhiteSpace(description))
                return new() { success = false, statusCode = 400, message = "Description requise." };

            if (await _provider.existsByLabelAsync(label, ct))
                return new() { success = false, statusCode = 409, message = "Un EffectDescription avec ce label existe déjà." };

            EffectDescription entity = await _provider.addAsync(new EffectDescription
            {
                Id = Guid.NewGuid(),
                Label = label,
                Description = description,
                Parameter = string.IsNullOrWhiteSpace(input.Parameter) ? null : input.Parameter!.Trim()
            }, ct);

            return new() { success = true, statusCode = 201, data = Map(entity) };
        }

        public async Task<ResultDTO<EffectDescriptionDto>> updateAsync(Guid id, EffectDescriptionInputDto input, CancellationToken ct = default)
        {
            EffectDescription current = await _provider.findByIdAsync(id, ct);
            if (current is null)
                return new() { success = false, statusCode = 404, message = "EffectDescription introuvable." };

            string label = N(input.Label);
            string description = N(input.Description);

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
            int inUse = await _provider.countEffectsUsingAsync(id, ct);
            if (inUse > 0)
                return new() { success = false, statusCode = 409, message = "Impossible de supprimer : description utilisée par des effets." };

            bool ok = await _provider.deleteAsync(id, ct);
            if (!ok) return new() { success = false, statusCode = 404, message = "EffectDescription introuvable." };

            return new() { success = true, statusCode = 200, data = true };
        }
    }
}
