using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace VortexTCG.DataAccess
{
    public class VortexDbContextFactory : IDesignTimeDbContextFactory<VortexDbContext>
    {
        public VortexDbContext CreateDbContext(string[] args)
        {
            // Récupérer les variables d'environnement
            var server = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "3307";
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "vortexdb";
            var username = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "Val";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "mdpVal";

            // Construire la connection string
            var connectionString = $"Server={server};Port={port};Database={database};User={username};Password={password};";

            var optionsBuilder = new DbContextOptionsBuilder<VortexDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            // Pour la factory, IHttpContextAccessor peut être null
            return new VortexDbContext(optionsBuilder.Options, httpContextAccessor: null);
        }
    }
}
