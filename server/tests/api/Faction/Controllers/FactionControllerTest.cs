using Microsoft.AspNetCore.Mvc;
using VortexTCG.Faction.Controllers;
using VortexTCG.Faction.Providers;
using VortexTCG.Faction.Services;
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
            FactionProvider provider = new FactionProvider(db);
            FactionService service = new FactionService(provider);
            FactionController controller = new FactionController(service);

            await createFaction(db);

            var result = await controller.GetAllFactions();
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<List<FactionDto>> payload = Assert.IsType<ResultDTO<List<FactionDto>>>(okResult.Value);
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
            FactionProvider provider = new FactionProvider(db);
            FactionService service = new FactionService(provider);
            FactionController controller = new FactionController(service);

            var factionId = await createFaction(db);

            var result = await controller.GetFactionWithCardsById(factionId);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<FactionWithCardsDto> payload = Assert.IsType<ResultDTO<FactionWithCardsDto>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(payload);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
        }

    }
}