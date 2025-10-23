using VortexTCG.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace VortexTCG.Common.Services
{

    public class VortexDbCoontextFactory
    {
        static public VortexDbContext getInMemoryDbContext()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VortexDbContext(options);
        }
    }
}