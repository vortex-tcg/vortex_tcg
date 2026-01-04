using System;
using System.Linq;
using api.Effect.Providers;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Common.Services;
using Xunit;

namespace Tests
{
    public class EffectDescriptionProviderTests
    {
        [Fact]
        public async Task Add_List_Find_Exists()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            EffectDescriptionProvider provider = new EffectDescriptionProvider(db);

            EffectDescription entity = new EffectDescription
            {
                Id = Guid.NewGuid(),
                Label = "Desc A",
                Description = "Details",
                Parameter = "P"
            };

            await provider.addAsync(entity);

            Assert.True(await provider.existsByLabelAsync("Desc A"));
            EffectDescription? found = await provider.findByIdAsync(entity.Id);
            Assert.NotNull(found);

            System.Collections.Generic.List<EffectDescription> list = await provider.listAsync();
            Assert.Single(list);
        }

        [Fact]
        public async Task Update_Success()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            EffectDescriptionProvider provider = new EffectDescriptionProvider(db);

            Guid id = Guid.NewGuid();
            EffectDescription entity = new EffectDescription
            {
                Id = id,
                Label = "Original",
                Description = "Desc",
                Parameter = null
            };
            
            db.EffectDescriptions.Add(entity);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();

            entity.Label = "Updated";
            bool updated = await provider.updateAsync(entity);
            Assert.True(updated);
        }

        [Fact]
        public async Task Delete_Success()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            EffectDescriptionProvider provider = new EffectDescriptionProvider(db);

            Guid id = Guid.NewGuid();
            await provider.addAsync(new EffectDescription
            {
                Id = id,
                Label = "ToDelete",
                Description = "Desc",
                Parameter = null
            });

            bool deleted = await provider.deleteAsync(id);
            Assert.True(deleted);
            Assert.Equal(0, db.EffectDescriptions.Count());
        }

        [Fact]
        public async Task Delete_ReturnsFalse_WhenMissing()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            EffectDescriptionProvider provider = new EffectDescriptionProvider(db);

            bool deleted = await provider.deleteAsync(Guid.NewGuid());
            Assert.False(deleted);
        }

        [Fact]
        public async Task CountEffectsUsing_Returns_Number_Of_References()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            EffectDescriptionProvider provider = new EffectDescriptionProvider(db);

            Guid descriptionId = Guid.NewGuid();
            await provider.addAsync(new EffectDescription
            {
                Id = descriptionId,
                Label = "InUse",
                Description = "Desc",
                Parameter = null
            });

            db.Effects.Add(new VortexTCG.DataAccess.Models.Effect
            {
                Id = Guid.NewGuid(),
                Title = "Effect",
                Parameter = "p",
                EffectTypeId = Guid.NewGuid(),
                EffectDescriptionId = descriptionId,
                StartConditionId = Guid.NewGuid(),
                EndConditionId = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test",
                UpdatedAtUtc = DateTime.UtcNow,
                UpdatedBy = "test"
            });
            await db.SaveChangesAsync();

            int count = await provider.countEffectsUsingAsync(descriptionId);
            Assert.Equal(1, count);
        }
    }
}
