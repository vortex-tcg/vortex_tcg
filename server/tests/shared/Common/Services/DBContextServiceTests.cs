using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using Xunit;

namespace VortexTCG.Tests.Shared.Common.Services
{
    public class DBContextServiceTests
    {
        [Fact]
        public void GetInMemoryDbContext_ReturnsNonNull()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();

            Assert.NotNull(db);
        }

        [Fact]
        public void GetInMemoryDbContext_ReturnsIndependentInstances()
        {
            using VortexDbContext db1 = VortexDbCoontextFactory.getInMemoryDbContext();
            using VortexDbContext db2 = VortexDbCoontextFactory.getInMemoryDbContext();

            Assert.NotSame(db1, db2);
        }

        [Fact]
        public async Task GetInMemoryDbContext_IsolatesData_BetweenInstances()
        {
            using VortexDbContext db1 = VortexDbCoontextFactory.getInMemoryDbContext();
            using VortexDbContext db2 = VortexDbCoontextFactory.getInMemoryDbContext();

            db1.Ranks.Add(new VortexTCG.DataAccess.Models.Rank { Id = Guid.NewGuid(), Label = "Gold", nbVictory = 10 });
            await db1.SaveChangesAsync();

            Assert.Equal(1, db1.Ranks.Count());
            Assert.Equal(0, db2.Ranks.Count());
        }
    }
}