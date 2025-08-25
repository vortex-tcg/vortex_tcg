using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace VortexTCG.DataAccess
{
    public class VortexDbContextFactory : IDesignTimeDbContextFactory<VortexDbContext>
    {
        public VortexDbContext CreateDbContext(string[] args)
        {
            DotNetEnv.Env.Load("../../.env");
            
            // Récupérer les variables d'environnement
            var server = Environment.GetEnvironmentVariable("DB_HOST");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var database = Environment.GetEnvironmentVariable("DB_NAME");
            var username = Environment.GetEnvironmentVariable("DB_USERNAME");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            // Construire la connection string
            var connectionString = $"Server={server};Port={port};Database={database};User={username};Password={password};";

            var optionsBuilder = new DbContextOptionsBuilder<VortexDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            // Pour la factory, IHttpContextAccessor peut être null
            return new VortexDbContext(optionsBuilder.Options, httpContextAccessor: null);
        }
    }
}
