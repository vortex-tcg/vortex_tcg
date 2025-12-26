using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using RankModel = VortexTCG.DataAccess.Models.Rank;

namespace VortexTCG.Api.Rank.Providers
{
    public class RankProvider
    {
        private readonly VortexDbContext _db;
        public RankProvider(VortexDbContext db)
        {
            _db = db;
        }

        public async Task<List<RankModel>> GetAllAsync()
        {
            return await _db.Ranks.ToListAsync();
        }

        public async Task<RankModel?> GetByIdAsync(Guid id)
        {
            return await _db.Ranks.FindAsync(id);
        }

        public async Task<RankModel> AddAsync(RankModel rank)
        {
            _db.Ranks.Add(rank);
            await _db.SaveChangesAsync();
            return rank;
        }

        public async Task<bool> UpdateAsync(RankModel rank)
        {
            var existing = await _db.Ranks.FindAsync(rank.Id);
            if (existing == null) return false;
            existing.Label = rank.Label;
            existing.nbVictory = rank.nbVictory;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var rank = await _db.Ranks.FindAsync(id);
            if (rank == null) return false;
            _db.Ranks.Remove(rank);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
