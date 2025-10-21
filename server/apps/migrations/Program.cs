using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;

Console.WriteLine("🔄 Début de l'application des migrations...");

try
{
    // Récupère la connection string depuis les variables d'environnement
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("ConnectionString manquante !");
        Environment.Exit(1);
    }

    Console.WriteLine($"Connexion à la base de données...");

    // Configure le DbContext
    var optionsBuilder = new DbContextOptionsBuilder<VortexDbContext>();
    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

    using var db = new VortexDbContext(optionsBuilder.Options);

    // Vérifie la connexion
    if (!db.Database.CanConnect())
    {
        Console.WriteLine("Impossible de se connecter à la base de données !");
        Environment.Exit(1);
    }

    Console.WriteLine("Connexion établie");

    // Récupère les migrations en attente
    var pendingMigrations = db.Database.GetPendingMigrations().ToList();

    if (pendingMigrations.Any())
    {
        Console.WriteLine($"{pendingMigrations.Count} migration(s) en attente :");
        foreach (var migration in pendingMigrations)
        {
            Console.WriteLine($"   - {migration}");
        }

        Console.WriteLine("Application des migrations...");
        db.Database.Migrate();
        Console.WriteLine("Migrations appliquées avec succès !");
    }
    else
    {
        Console.WriteLine("Base de données déjà à jour, aucune migration nécessaire.");
    }

    Console.WriteLine("Terminé !");
    Environment.Exit(0);
}
catch (Exception ex)
{
    Console.WriteLine($"Erreur lors de l'application des migrations : {ex.Message}");
    Console.WriteLine($"Stack trace : {ex.StackTrace}");
    Environment.Exit(1);
}