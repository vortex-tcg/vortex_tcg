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
        private (FactionController controller, VortexDbContext db) createController()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            return (new FactionController(db, config), db);
        }

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
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            FactionController controller = new FactionController(db, config);

            var factionId = await createFaction(db);

            var result = await controller.GetFactionWithCardsById(factionId);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<FactionWithCardsDto> payload = Assert.IsType<ResultDTO<FactionWithCardsDto>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(payload);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
        }

        [Fact(DisplayName = "Get faction by ID")]
        public async Task getFactionByIdReturnsFaction()
        {
            var (controller, db) = createController();
            var factionId = await createFaction(db);

            var result = await controller.GetFactionById(factionId);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<FactionDto> payload = Assert.IsType<ResultDTO<FactionDto>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(payload.data);
            Assert.Equal(factionId, payload.data!.Id);
        }

        [Fact(DisplayName = "Get faction with champions by ID")]
        public async Task getFactionWithChampionsById()
        {
            var (controller, db) = createController();
            var factionId = await createFaction(db);

            var result = await controller.GetFactionWithChampionsById(factionId);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<FactionWithChampionDto> payload = Assert.IsType<ResultDTO<FactionWithChampionDto>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(payload.data);
            Assert.Equal(factionId, payload.data!.Id);
        }

        [Fact(DisplayName = "Create faction")]
        public async Task createFactionReturnsCreated()
        {
            var (controller, _) = createController();
            var dto = new CreateFactionDto
            {
                Label = "NewFaction",
                Currency = "Silver",
                Condition = "None"
            };

            var result = await controller.CreateFaction(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(result);
            ResultDTO<FactionDto> payload = Assert.IsType<ResultDTO<FactionDto>>(created.Value);
            Assert.Equal(201, created.StatusCode);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
            Assert.Equal("NewFaction", payload.data!.Label);
        }

        [Fact(DisplayName = "Update faction")]
        public async Task updateFactionReturnsUpdated()
        {
            var (controller, db) = createController();
            var factionId = await createFaction(db);
            var dto = new UpdateFactionDto
            {
                Label = "UpdatedFaction",
                Currency = "Bronze",
                Condition = "Updated"
            };

            var result = await controller.UpdateFaction(factionId, dto);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<FactionDto> payload = Assert.IsType<ResultDTO<FactionDto>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
            Assert.Equal("UpdatedFaction", payload.data!.Label);
            Assert.Equal("Bronze", payload.data.Currency);
        }

        [Fact(DisplayName = "Delete faction")]
        public async Task deleteFactionReturnsOk()
        {
            var (controller, db) = createController();
            var factionId = await createFaction(db);

            var result = await controller.DeleteFaction(factionId);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<object> payload = Assert.IsType<ResultDTO<object>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
            Assert.True(payload.success);
        }

        [Fact(DisplayName = "Delete faction not found")]
        public async Task deleteFactionReturnsNotFound()
        {
            var (controller, _) = createController();

            var result = await controller.DeleteFaction(Guid.NewGuid());
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<object> payload = Assert.IsType<ResultDTO<object>>(okResult.Value);
            Assert.Equal(404, okResult.StatusCode);
            Assert.False(payload.success);
        }

    }
}