using VortexTCG.Api.Logs.GameLog.Providers;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using GameLogModel = VortexTCG.DataAccess.Models.Gamelog;
using ActionTypeModel = VortexTCG.DataAccess.Models.ActionType;
using Xunit;

namespace VortexTCG.Tests.Api.Log.GameLog.Providers
{
    public class GameLogProviderTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        [Fact]
        public void Query_ReturnsQueryable()
        {
            using VortexDbContext db = CreateDb();
            GameLogProvider provider = new GameLogProvider(db);

            var query = provider.Query();

            Assert.NotNull(query);
        }

        [Fact]
        public void Query_ReturnsQueryable_WithData()
        {
            using VortexDbContext db = CreateDb();
            db.Gamelogs.Add(new GameLogModel { Id = Guid.NewGuid(), Label = "L", TurnNumber = 1 });
            db.SaveChanges();
            GameLogProvider provider = new GameLogProvider(db);

            var result = provider.Query().ToList();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            GameLogProvider provider = new GameLogProvider(db);

            GameLogModel? result = await provider.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsGameLog_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            Guid id = Guid.NewGuid();
            db.Gamelogs.Add(new GameLogModel { Id = id, Label = "MyLog", TurnNumber = 5 });
            await db.SaveChangesAsync();
            GameLogProvider provider = new GameLogProvider(db);

            GameLogModel? result = await provider.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("MyLog", result!.Label);
            Assert.Equal(5, result.TurnNumber);
        }

        [Fact]
        public async Task GetByIdAsync_IncludesActions()
        {
            using VortexDbContext db = CreateDb();
            Guid gamelogId = Guid.NewGuid();
            Guid actionId = Guid.NewGuid();
            GameLogModel gamelog = new GameLogModel { Id = gamelogId, Label = "G", TurnNumber = 1 };
            db.Gamelogs.Add(gamelog);
            ActionTypeModel action = new ActionTypeModel { Id = actionId, actionDescription = "Act", GameLogId = gamelogId };
            db.Actions.Add(action);
            await db.SaveChangesAsync();
            GameLogProvider provider = new GameLogProvider(db);

            GameLogModel? result = await provider.GetByIdAsync(gamelogId);

            Assert.NotNull(result);
            Assert.NotNull(result!.Actions);
            Assert.Contains(result.Actions!, a => a.Id == actionId);
        }

        [Fact]
        public async Task AddAsync_PersistsGameLog()
        {
            using VortexDbContext db = CreateDb();
            GameLogProvider provider = new GameLogProvider(db);
            GameLogModel entity = new GameLogModel { Id = Guid.NewGuid(), Label = "New", TurnNumber = 3 };

            await provider.AddAsync(entity);

            Assert.Equal(1, db.Gamelogs.Count());
            Assert.Equal("New", db.Gamelogs.Single().Label);
        }

        [Fact]
        public async Task UpdateAsync_ModifiesGameLog()
        {
            using VortexDbContext db = CreateDb();
            GameLogModel entity = new GameLogModel { Id = Guid.NewGuid(), Label = "Old", TurnNumber = 1 };
            db.Gamelogs.Add(entity);
            await db.SaveChangesAsync();
            GameLogProvider provider = new GameLogProvider(db);
            entity.Label = "Updated";
            entity.TurnNumber = 42;

            await provider.UpdateAsync(entity);

            GameLogModel? updated = db.Gamelogs.Single();
            Assert.Equal("Updated", updated.Label);
            Assert.Equal(42, updated.TurnNumber);
        }

        [Fact]
        public async Task DeleteAsync_RemovesGameLog()
        {
            using VortexDbContext db = CreateDb();
            GameLogModel entity = new GameLogModel { Id = Guid.NewGuid(), Label = "Del", TurnNumber = 1 };
            db.Gamelogs.Add(entity);
            await db.SaveChangesAsync();
            GameLogProvider provider = new GameLogProvider(db);

            await provider.DeleteAsync(entity);

            Assert.Equal(0, db.Gamelogs.Count());
        }
    }
}
