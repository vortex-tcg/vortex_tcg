using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using CardModel = VortexTCG.DataAccess.Models.Card;

namespace VortexTCG.Api.Card.Providers
{
    public class CardProvider
    {
        private readonly VortexDbContext _db;
        public CardProvider(VortexDbContext db) => _db = db;

        public Task<List<CardModel>> GetAllAsync(CancellationToken ct = default)
        => _db.Cards.AsNoTracking().OrderBy(e => e.Name).ToListAsync(ct);

        public async Task<CardModel> AddAsync(CardModel entity, CancellationToken ct = default)
        {
            await _db.Cards.AddAsync(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _db.Cards.AnyAsync(e => e.Name == name, ct);

        public Task<CardModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
    }
}