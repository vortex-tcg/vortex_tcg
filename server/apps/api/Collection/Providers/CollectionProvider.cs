using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using CollectionModel = VortexTCG.DataAccess.Models.Collection;

namespace VortexTCG.Api.Collection.Providers
{
    public class CollectionProvider
    {
        private readonly VortexDbContext _db;

        public CollectionProvider(VortexDbContext db)
        {
            _db = db;
        }

        public async Task<List<CollectionModel>> GetAllAsync()
            => await _db.Collections
                .Include(c => c.User)
                .Include(c => c.Cards)
                    .ThenInclude(cc => cc.Card)
                        .ThenInclude(card => card.Class)
                .Include(c => c.Cards)
                    .ThenInclude(cc => cc.Card)
                        .ThenInclude(card => card.Factions)
                .Include(c => c.Champions)
                    .ThenInclude(ch => ch.Champion)
                .ToListAsync();

        public async Task<CollectionModel?> GetByIdAsync(Guid id)
            => await _db.Collections
                .Include(c => c.User)
                .Include(c => c.Cards)
                    .ThenInclude(cc => cc.Card)
                        .ThenInclude(card => card.Class)
                .Include(c => c.Cards)
                    .ThenInclude(cc => cc.Card)
                        .ThenInclude(card => card.Factions)
                .Include(c => c.Champions)
                    .ThenInclude(ch => ch.Champion)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<CollectionModel?> GetByUserIdAsync(Guid userId)
            => await _db.Collections
                .Include(c => c.User)
                .Include(c => c.Cards)
                    .ThenInclude(cc => cc.Card)
                        .ThenInclude(card => card.Class)
                .Include(c => c.Cards)
                    .ThenInclude(cc => cc.Card)
                        .ThenInclude(card => card.Factions)
                .Include(c => c.Champions)
                    .ThenInclude(ch => ch.Champion)
                .FirstOrDefaultAsync(c => c.User.Id == userId);

        public async Task<CollectionModel> AddAsync(CollectionModel collection)
        {
            if (collection.User != null)
            {
                var existingUser = await _db.Users.FindAsync(collection.User.Id);

                if (existingUser != null)
                {
                    collection.User = existingUser;
                }
                else
                {
                    _db.Entry(collection.User).State = EntityState.Detached;
                    collection.User = null!;
                }
            }

            await _db.Collections.AddAsync(collection);
            await _db.SaveChangesAsync();
            return collection;
        }
        public virtual async Task<bool> UpdateAsync(CollectionModel collection)
        {
            CollectionModel? existing = await _db.Collections
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == collection.Id);

            if (existing == null)
                return false;

            if (collection.User != null)
            {
                var existingUser = await _db.Users.FindAsync(collection.User.Id);

                if (existingUser != null)
                {
                    existing.User = existingUser;
                }
                else
                {
                    existing.User = null!;
                }
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            CollectionModel? collection = await _db.Collections.FindAsync(id);
            if (collection == null) return false;

            _db.Collections.Remove(collection);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}