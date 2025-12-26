using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using DataFaction = VortexTCG.DataAccess.Models.Faction;

namespace VortexTCG.Faction.Providers {

    public class FactionProvider
    {
        private readonly VortexDbContext _db;

        public FactionProvider(VortexDbContext db)
        {
            _db = db;
        }

        public async Task<(bool IsValid, List<Guid> InvalidIds)> ValidateCardIds(List<Guid> cardIds)
        {
            if (cardIds == null || !cardIds.Any())
                return (true, new List<Guid>());

            var existingCardIds = await _db.Cards
                .Where(c => cardIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            var invalidIds = cardIds.Except(existingCardIds).ToList();
            return (invalidIds.Count == 0, invalidIds);
        }

        public async Task<bool> ValidateChampionId(Guid championId)
        {
            if (championId == Guid.Empty)
                return true;

            return await _db.Champions.AnyAsync(ch => ch.Id == championId);
        }

        public Task<List<DataFaction>> GetAllAsync(CancellationToken ct = default)
        => _db.Factions.AsNoTracking().ToListAsync(ct);

        public Task<DataFaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Factions.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct);

        public Task<DataFaction?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default)
        => _db.Factions.FirstOrDefaultAsync(f => f.Id == id, ct);

        public Task<DataFaction?> GetWithCardsAsync(Guid id, CancellationToken ct = default)
        => _db.Factions
            .AsNoTracking()
            .Include(f => f.Cards)
                .ThenInclude(fc => fc.Card)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        public Task<DataFaction?> GetWithChampionAsync(Guid id, CancellationToken ct = default)
        => _db.Factions
            .AsNoTracking()
            .Include(f => f.Champions)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        public async Task<bool> DeleteFaction(Guid id)
        {
            var faction = await _db.Factions.FindAsync(id);
            if (faction == null)
                return false;

            _db.Factions.Remove(faction);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<DataFaction> AddAsync(DataFaction entity, CancellationToken ct = default)
        {
            await _db.Factions.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<DataFaction> UpdateAsync(DataFaction entity, CancellationToken ct = default)
        {
            _db.Factions.Update(entity);
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        public async Task ReplaceCardsAsync(Guid factionId, IEnumerable<Guid> cardIds, CancellationToken ct = default)
        {
            List<FactionCard> existingFactionCards = await _db.FactionCards
                .Where(fc => fc.FactionId == factionId)
                .ToListAsync(ct);
            _db.FactionCards.RemoveRange(existingFactionCards);

            foreach (Guid cardId in cardIds)
            {
                FactionCard factionCard = new FactionCard
                {
                    Id = Guid.NewGuid(),
                    FactionId = factionId,
                    CardId = cardId,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedAtUtc = DateTime.UtcNow,
                    UpdatedBy = "System"
                };
                _db.FactionCards.Add(factionCard);
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task SetChampionAsync(Guid factionId, Guid? championId, CancellationToken ct = default)
        {
            Champion? oldChampion = await _db.Champions.FirstOrDefaultAsync(c => c.FactionId == factionId, ct);
            if (oldChampion != null)
            {
                oldChampion.FactionId = Guid.Empty;
                oldChampion.UpdatedAtUtc = DateTime.UtcNow;
                oldChampion.UpdatedBy = "System";
            }

            if (championId.HasValue && championId.Value != Guid.Empty)
            {
                Champion? newChampion = await _db.Champions.FirstOrDefaultAsync(c => c.Id == championId.Value, ct);
                if (newChampion != null)
                {
                    newChampion.FactionId = factionId;
                    newChampion.UpdatedAtUtc = DateTime.UtcNow;
                    newChampion.UpdatedBy = "System";
                }
            }

            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> FactionExists(Guid id)
        {
            return await _db.Factions.AnyAsync(f => f.Id == id);
        }


        public async Task<bool> LabelExists(string label, Guid? excludeId = null)
        {
            return await _db.Factions
                           .Where(f => f.Label == label && (excludeId == null || f.Id != excludeId))
                           .AnyAsync();
        }
    }
}