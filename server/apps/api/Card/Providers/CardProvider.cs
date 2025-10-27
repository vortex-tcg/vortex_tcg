using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using CardModel = VortexTCG.DataAccess.Models.Card;

namespace api.Card.Providers
{
    public class CardProvider
    {
        private readonly VortexDbContext _db;
        public CardProvider(VortexDbContext db) => _db = db;

        public async Task<CardModel> addAsync(CardModel entity, CancellationToken ct = default)
        {
            _db.Cards.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<bool> existsByNameAsync(string name, CancellationToken ct = default)
        {
            return await _db.Cards.AnyAsync(e => e.Name == name, ct);
        }
    }
}