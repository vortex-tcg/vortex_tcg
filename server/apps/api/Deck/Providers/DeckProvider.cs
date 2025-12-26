using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using DeckModel = VortexTCG.DataAccess.Models.Deck;

namespace VortexTCG.Api.Deck.Providers
{
    public class DeckProvider
    {
        private readonly VortexDbContext _db;

        public DeckProvider(VortexDbContext db) => _db = db;

        public Task<List<DeckModel>> GetAllAsync(CancellationToken ct = default)
        => _db.Decks.AsNoTracking().OrderBy(e => e.Label).ToListAsync(ct);

        public Task<DeckModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Decks.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

        public Task<DeckModel?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default)
        => _db.Decks.FirstOrDefaultAsync(d => d.Id == id, ct);

        public async Task<DeckModel> AddAsync(DeckModel entity, CancellationToken ct = default)
        {
            await _db.Decks.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<DeckModel> UpdateAsync(DeckModel entity, CancellationToken ct = default)
        {
            _db.Decks.Update(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task DeleteAsync(DeckModel entity, CancellationToken ct = default)
        {
            _db.Decks.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public Task<bool> ExistsByLabelAndUserAsync(string label, Guid userId, CancellationToken ct = default)
        => _db.Decks.AnyAsync(d => d.Label == label && d.UserId == userId, ct);

        public Task<List<DeckModel>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _db.Decks.AsNoTracking().Where(d => d.UserId == userId).OrderBy(d => d.Label).ToListAsync(ct);
    }
}
