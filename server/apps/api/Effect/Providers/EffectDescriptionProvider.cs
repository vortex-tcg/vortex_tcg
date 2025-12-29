using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace api.Effect.Providers
{
    public class EffectDescriptionProvider
    {
        private readonly VortexDbContext _db;
        public EffectDescriptionProvider(VortexDbContext db) => _db = db;

        public Task<List<EffectDescription>> listAsync(CancellationToken ct = default) =>
            _db.EffectDescriptions.AsNoTracking().OrderBy(e => e.Label).ToListAsync(ct);

        public Task<EffectDescription?> findByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.EffectDescriptions.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

        public Task<bool> existsByLabelAsync(string label, CancellationToken ct = default) =>
            _db.EffectDescriptions.AnyAsync(e => e.Label == label, ct);

        public Task<int> countEffectsUsingAsync(Guid effectDescriptionId, CancellationToken ct = default) =>
            _db.Effects.CountAsync(x => x.EffectDescriptionId == effectDescriptionId, ct);

        public async Task<EffectDescription> addAsync(EffectDescription entity, CancellationToken ct = default)
        {
            await _db.EffectDescriptions.AddAsync(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> updateAsync(EffectDescription entity, CancellationToken ct = default)
        {
            _db.EffectDescriptions.Update(entity);
            return (await _db.SaveChangesAsync(ct)) > 0;
        }

        public async Task<bool> deleteAsync(Guid id, CancellationToken ct = default)
        {
            EffectDescription tracked = await _db.EffectDescriptions.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (tracked is null) return false;
            _db.EffectDescriptions.Remove(tracked);
            return (await _db.SaveChangesAsync(ct)) > 0;
        }
    }
}