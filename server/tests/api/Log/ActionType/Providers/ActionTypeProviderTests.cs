using VortexTCG.Api.Logs.ActionType.Providers;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using ActionTypeModel = VortexTCG.DataAccess.Models.ActionType;
using GamelogModel = VortexTCG.DataAccess.Models.Gamelog;
using Xunit;

namespace VortexTCG.Tests.Api.Log.ActionType.Providers
{
    public class ActionTypeProviderTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        [Fact]
        public void Query_ReturnsQueryable()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeProvider provider = new ActionTypeProvider(db);

            var query = provider.Query();

            Assert.NotNull(query);
        }

        [Fact]
        public void Query_ReturnsQueryable_WithData()
        {
            using VortexDbContext db = CreateDb();
            db.Actions.Add(new ActionTypeModel { Id = Guid.NewGuid(), actionDescription = "A" });
            db.SaveChanges();
            ActionTypeProvider provider = new ActionTypeProvider(db);

            var result = provider.Query().ToList();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeProvider provider = new ActionTypeProvider(db);

            ActionTypeModel? result = await provider.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsActionType_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            Guid id = Guid.NewGuid();
            db.Actions.Add(new ActionTypeModel { Id = id, actionDescription = "TestAction" });
            await db.SaveChangesAsync();
            ActionTypeProvider provider = new ActionTypeProvider(db);

            ActionTypeModel? result = await provider.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("TestAction", result!.actionDescription);
        }

        [Fact]
        public async Task GetByIdAsync_IncludesParentAndChilds()
        {
            using VortexDbContext db = CreateDb();
            Guid parentId = Guid.NewGuid();
            Guid childId = Guid.NewGuid();
            ActionTypeModel parent = new ActionTypeModel { Id = parentId, actionDescription = "Parent", ParentId = parentId };
            ActionTypeModel child = new ActionTypeModel { Id = childId, actionDescription = "Child", ParentId = parentId };
            db.Actions.AddRange(parent, child);
            await db.SaveChangesAsync();
            ActionTypeProvider provider = new ActionTypeProvider(db);

            ActionTypeModel? result = await provider.GetByIdAsync(parentId);

            Assert.NotNull(result);
            Assert.NotNull(result!.Childs);
            Assert.Contains(result.Childs, c => c.Id == childId);
        }

        [Fact]
        public async Task AddAsync_PersistsActionType()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeProvider provider = new ActionTypeProvider(db);
            ActionTypeModel entity = new ActionTypeModel { Id = Guid.NewGuid(), actionDescription = "New Action" };

            await provider.AddAsync(entity);

            Assert.Equal(1, db.Actions.Count());
            Assert.Equal("New Action", db.Actions.Single().actionDescription);
        }

        [Fact]
        public async Task UpdateAsync_ModifiesActionType()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeModel entity = new ActionTypeModel { Id = Guid.NewGuid(), actionDescription = "Old" };
            db.Actions.Add(entity);
            await db.SaveChangesAsync();
            ActionTypeProvider provider = new ActionTypeProvider(db);
            entity.actionDescription = "Updated";

            await provider.UpdateAsync(entity);

            Assert.Equal("Updated", db.Actions.Single().actionDescription);
        }

        [Fact]
        public async Task DeleteAsync_RemovesActionType()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeModel entity = new ActionTypeModel { Id = Guid.NewGuid(), actionDescription = "ToDelete" };
            db.Actions.Add(entity);
            await db.SaveChangesAsync();
            ActionTypeProvider provider = new ActionTypeProvider(db);

            await provider.DeleteAsync(entity);

            Assert.Equal(0, db.Actions.Count());
        }
    }
}