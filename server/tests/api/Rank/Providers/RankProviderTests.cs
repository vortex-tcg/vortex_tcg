using VortexTCG.Api.Rank.Providers;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using Xunit;
using RankModel = VortexTCG.DataAccess.Models.Rank;

namespace VortexTCG.Tests.Api.Rank.Providers
{
    public class RankProviderTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoRanks()
        {
            using VortexDbContext db = CreateDb();
            RankProvider provider = new RankProvider(db);

            List<RankModel> result = await provider.GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllRanks()
        {
            using VortexDbContext db = CreateDb();
            db.Ranks.Add(new RankModel { Id = Guid.NewGuid(), Label = "Bronze", nbVictory = 5 });
            db.Ranks.Add(new RankModel { Id = Guid.NewGuid(), Label = "Silver", nbVictory = 10 });
            await db.SaveChangesAsync();

            RankProvider provider = new RankProvider(db);
            List<RankModel> result = await provider.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            RankProvider provider = new RankProvider(db);

            RankModel? result = await provider.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsRank_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            RankModel rank = new RankModel { Id = Guid.NewGuid(), Label = "Gold", nbVictory = 20 };
            db.Ranks.Add(rank);
            await db.SaveChangesAsync();

            RankProvider provider = new RankProvider(db);
            RankModel? result = await provider.GetByIdAsync(rank.Id);

            Assert.NotNull(result);
            Assert.Equal("Gold", result!.Label);
        }

        [Fact]
        public async Task AddAsync_PersistsRank()
        {
            using VortexDbContext db = CreateDb();
            RankProvider provider = new RankProvider(db);
            RankModel rank = new RankModel { Id = Guid.NewGuid(), Label = "Master", nbVictory = 50 };

            RankModel added = await provider.AddAsync(rank);

            Assert.Equal(rank.Id, added.Id);
            Assert.Equal(1, db.Ranks.Count());
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFalse_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            RankProvider provider = new RankProvider(db);
            RankModel rank = new RankModel { Id = Guid.NewGuid(), Label = "Ghost", nbVictory = 0 };

            bool result = await provider.UpdateAsync(rank);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsTrue_AndUpdates()
        {
            using VortexDbContext db = CreateDb();
            RankModel rank = new RankModel { Id = Guid.NewGuid(), Label = "Old", nbVictory = 1 };
            db.Ranks.Add(rank);
            await db.SaveChangesAsync();

            RankProvider provider = new RankProvider(db);
            bool result = await provider.UpdateAsync(new RankModel { Id = rank.Id, Label = "New", nbVictory = 99 });

            Assert.True(result);
            RankModel? updated = await provider.GetByIdAsync(rank.Id);
            Assert.Equal("New", updated!.Label);
            Assert.Equal(99, updated.nbVictory);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            RankProvider provider = new RankProvider(db);

            bool result = await provider.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_AndRemoves()
        {
            using VortexDbContext db = CreateDb();
            RankModel rank = new RankModel { Id = Guid.NewGuid(), Label = "Del", nbVictory = 1 };
            db.Ranks.Add(rank);
            await db.SaveChangesAsync();

            RankProvider provider = new RankProvider(db);
            bool result = await provider.DeleteAsync(rank.Id);

            Assert.True(result);
            Assert.Equal(0, db.Ranks.Count());
        }
    }
}