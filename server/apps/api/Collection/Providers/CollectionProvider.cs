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
        {
            return await _db.Collections.ToListAsync();
        }

        public async Task<CollectionModel?> GetByIdAsync(Guid id)
        {
            return await _db.Collections.FindAsync(id);
        }

        public async Task<CollectionModel> AddAsync(CollectionModel collection)
        {
            _db.Collections.Add(collection);
            await _db.SaveChangesAsync();
            return collection;
        }

        public async Task<bool> UpdateAsync(CollectionModel collection)
        {
            var existing = await _db.Collections.FindAsync(collection.Id);
            if (existing == null) return false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var collection = await _db.Collections.FindAsync(id);
            if (collection == null) return false;
            _db.Collections.Remove(collection);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
