using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VortexTCG.Faction.Controllers;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Common.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Auth.DTOs;
using Scrypt;
using VortexTCG.Faction.DTOs;

namespace Tests

{
    public class FactionControllerTest
    {
        private async Task<Guid> createFaction(VortexDbContext db)
        {
            var factionId = Guid.NewGuid();
            db.Factions.Add(new VortexTCG.DataAccess.Models.Faction
            {
                Id = factionId,
                Label = "FactionA",
                Currency = "Gold",
                Condition = "None",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedAtUtc = DateTime.UtcNow,
                UpdatedBy = "System"
            });
            await db.SaveChangesAsync();
            return factionId;
        }

        [Fact(DisplayName = "Get all factions")]
        public async Task getAllFactionsReturnsFactions()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            FactionController controller = new FactionController(db, config);

            await createFaction(db);

            var result = await controller.GetAllFactions();
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<List<FactionDTO>> payload = Assert.IsType<ResultDTO<List<FactionDTO>>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(payload);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
            Assert.Single(payload.data);

            Console.WriteLine($"Retrieved Faction ID: {payload.data[0].Id}, Label: {payload.data[0].Label}");
        }

        [Fact(DisplayName = "Get faction with cards by ID")]
        public async Task getFactionWithCardsById()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            FactionController controller = new FactionController(db, config);

            var factionId = await createFaction(db);

            var result = await controller.GetFactionWithCardsById(factionId);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<FactionWithCardsDTO> payload = Assert.IsType<ResultDTO<FactionWithCardsDTO>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(payload);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
        }

    }
}