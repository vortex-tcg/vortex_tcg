using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using VortexTCG.DataAccess;

public class VortexDbContextFactory : IDesignTimeDbContextFactory<VortexDbContext>
{
    public VortexDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VortexDbContext>();
        
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
                               ?? "Server=db;Database=vortex_design;User=root;Password=;";

        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.Parse("11.4.5-mariadb")
        );

        return new VortexDbContext(optionsBuilder.Options);
    }
}