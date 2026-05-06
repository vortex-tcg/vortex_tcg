using VortexTCG.Api.Card.Providers;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using Xunit;
using CardModel = VortexTCG.DataAccess.Models.Card;

namespace VortexTCG.Tests.Api.Card.Providers
{
    public class CardProviderTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoCards()
        {
            using VortexDbContext db = CreateDb();
            CardProvider provider = new CardProvider(db);

            List<CardModel> result = await provider.GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCards_OrderedByName()
        {
            using VortexDbContext db = CreateDb();
            db.Cards.Add(new CardModel { Id = Guid.NewGuid(), Name = "Zebra", Price = 1, Description = "d", Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" });
            db.Cards.Add(new CardModel { Id = Guid.NewGuid(), Name = "Alpha", Price = 1, Description = "d", Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" });
            await db.SaveChangesAsync();

            CardProvider provider = new CardProvider(db);
            List<CardModel> result = await provider.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Alpha", result[0].Name);
            Assert.Equal("Zebra", result[1].Name);
        }

        [Fact]
        public async Task AddAsync_PersistsCard()
        {
            using VortexDbContext db = CreateDb();
            CardProvider provider = new CardProvider(db);
            CardModel card = new CardModel { Id = Guid.NewGuid(), Name = "NewCard", Price = 3, Description = "d", Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };

            CardModel added = await provider.AddAsync(card);

            Assert.Equal(card.Id, added.Id);
            Assert.Equal(1, db.Cards.Count());
        }

        [Fact]
        public async Task ExistsByNameAsync_ReturnsTrue_WhenExists()
        {
            using VortexDbContext db = CreateDb();
            db.Cards.Add(new CardModel { Id = Guid.NewGuid(), Name = "Existing", Price = 1, Description = "d", Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" });
            await db.SaveChangesAsync();

            CardProvider provider = new CardProvider(db);

            Assert.True(await provider.ExistsByNameAsync("Existing"));
            Assert.False(await provider.ExistsByNameAsync("Missing"));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            CardProvider provider = new CardProvider(db);

            CardModel? result = await provider.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCard_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            CardModel card = new CardModel { Id = Guid.NewGuid(), Name = "FoundCard", Price = 5, Description = "d", Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            db.Cards.Add(card);
            await db.SaveChangesAsync();

            CardProvider provider = new CardProvider(db);
            CardModel? result = await provider.GetByIdAsync(card.Id);

            Assert.NotNull(result);
            Assert.Equal("FoundCard", result!.Name);
        }
    }
}