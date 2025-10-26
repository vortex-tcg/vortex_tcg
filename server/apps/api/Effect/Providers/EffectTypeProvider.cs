using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace  api.Effect.Providers
{
    public class EffectTypeProvider
    {
        private readonly VortexDbContext _db;
        public EffectTypeProvider(VortexDbContext db) => _db = db;

        public Task<List<EffectType>> listAsync(CancellationToken ct = default) =>
            _db.EffectTypes.AsNoTracking().OrderBy(e => e.Label).ToListAsync(ct);

        public Task<EffectType?> findByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.EffectTypes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

        public Task<bool> existsByLabelAsync(string label, CancellationToken ct = default) =>
            _db.EffectTypes.AnyAsync(e => e.Label == label, ct);

        public async Task<EffectType> addAsync(EffectType entity, CancellationToken ct = default)
        {
            _db.EffectTypes.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> updateAsync(EffectType entity, CancellationToken ct = default)
        {
            _db.EffectTypes.Update(entity);
            return await _db.SaveChangesAsync(ct) > 0;
        }

        public async Task<bool> deleteAsync(Guid id, CancellationToken ct = default)
        {
            var tracked = await _db.EffectTypes.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (tracked is null) return false;
            _db.EffectTypes.Remove(tracked);
            return await _db.SaveChangesAsync(ct) > 0;
        }
    }
}
