using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using api.Effect.Providers;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.Api.Effect.Providers
{
    public class EffectTypeProviderTests
    {
        private static VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        [Fact]
        public async Task Add_List_Find_Exists()
        {
            using VortexDbContext db = CreateDb();
            EffectTypeProvider provider = new EffectTypeProvider(db);

            EffectType entity = new EffectType
            {
                Id = Guid.NewGuid(),
                Label = "Burn"
            };

            await provider.addAsync(entity);

            Assert.True(await provider.existsByLabelAsync("Burn"));
            EffectType? found = await provider.findByIdAsync(entity.Id);
            Assert.NotNull(found);

            var list = await provider.listAsync();
            Assert.Single(list);
        }

        [Fact]
        public async Task Update_Success()
        {
            using VortexDbContext db = CreateDb();
            EffectTypeProvider provider = new EffectTypeProvider(db);

            var id = Guid.NewGuid();
            var entity = new EffectType
            {
                Id = id,
                Label = "Original"
            };
            
            db.EffectTypes.Add(entity);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            entity.Label = "Updated";
            bool updated = await provider.updateAsync(entity);
            Assert.True(updated);
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            EffectTypeProvider provider = new EffectTypeProvider(db);

            bool deleted = await provider.deleteAsync(Guid.NewGuid());
            Assert.False(deleted);
        }

        [Fact]
        public async Task List_Returns_Sorted_By_Label()
        {
            using VortexDbContext db = CreateDb();
            EffectTypeProvider provider = new EffectTypeProvider(db);

            await provider.addAsync(new EffectType { Id = Guid.NewGuid(), Label = "Zap" });
            await provider.addAsync(new EffectType { Id = Guid.NewGuid(), Label = "Burn" });
            await provider.addAsync(new EffectType { Id = Guid.NewGuid(), Label = "Poison" });

            var list = await provider.listAsync();
            Assert.Equal(3, list.Count);
            Assert.Equal("Burn", list[0].Label);
            Assert.Equal("Poison", list[1].Label);
            Assert.Equal("Zap", list[2].Label);
        }

        [Fact]
        public async Task Delete_ReturnsTrue_WhenEntityExists()
        {
            using VortexDbContext db = CreateDb();
            EffectTypeProvider provider = new EffectTypeProvider(db);

            Guid id = Guid.NewGuid();
            await provider.addAsync(new EffectType { Id = id, Label = "Temp" });

            bool deleted = await provider.deleteAsync(id);
            Assert.True(deleted);
            Assert.Empty(db.EffectTypes);
        }
    }
}
