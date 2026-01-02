using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Card.DTOs;
using VortexTCG.Api.Card.Providers;
using VortexTCG.Api.Card.Services;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess;
using CardModel = VortexTCG.DataAccess.Models.Card;
using Xunit;

namespace Tests.Card.Services
{
    public class CardServiceTests
    {
        private static VortexDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private static CardService CreateService(VortexDbContext db)
        {
            var provider = new CardProvider(db);
            return new CardService(provider);
        }

        [Fact]
        public async Task Create_Returns400_WhenNameMissing()
        {
            using var db = CreateDb();
            var service = CreateService(db);
            var input = new CardCreateDto { Name = string.Empty, Price = 1, Hp = 1, Attack = 1, Description = "d", Picture = "p" };

            ResultDTO<CardDto> result = await service.CreateAsync(input);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task Create_Returns409_WhenDuplicate()
        {
            using var db = CreateDb();
            db.Cards.Add(new CardModel { Id = Guid.NewGuid(), Name = "Card", Price = 1, Description = "d", Picture = "p" });
            await db.SaveChangesAsync();
            var service = CreateService(db);
            var input = new CardCreateDto { Name = "Card", Price = 1, Hp = 1, Attack = 1, Description = "d", Picture = "p" };

            var result = await service.CreateAsync(input);

            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            using var db = CreateDb();
            var service = CreateService(db);
            var input = new CardCreateDto { Name = "New", Price = 1, Hp = 1, Attack = 1, Description = "d", Picture = "p" };

            var result = await service.CreateAsync(input);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.Equal("New", result.data!.Name);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenMissing()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            var result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task GetById_ReturnsData()
        {
            using var db = CreateDb();
            var entity = new CardModel { Id = Guid.NewGuid(), Name = "Card", Price = 1, Description = "d", Picture = "p" };
            db.Cards.Add(entity);
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var dto = await service.GetByIdAsync(entity.Id);

            Assert.NotNull(dto);
            Assert.True(dto.success);
            Assert.Equal("Card", dto.data!.Name);
        }
    }
}
