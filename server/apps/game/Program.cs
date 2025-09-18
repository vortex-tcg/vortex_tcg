// =============================================
// FICHIER: Program.cs (Minimal Hosting)
// Rôle: Point d'entrée de l'application ASP.NET Core.
//       Configure les services DI (SignalR, CORS, EF Core MySQL), le pipeline HTTP,
//       mappe le Hub "/hubs/game" et expose un endpoint /health/db.
// =============================================
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql; // Provider EF Core MySQL (Pomelo)
using System.Text.RegularExpressions;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Common.Services;
using game.Hubs;
using game.Services;

// Charge les variables d'environnement depuis un fichier .env (utile en dev/local).
Env.Load(Path.Combine(AppContext.BaseDirectory, "../../../../.env"));

var builder = WebApplication.CreateBuilder(args);

// 1) Services de base
builder.Services.AddSignalR(o => {
    o.EnableDetailedErrors = true;      // Logs d'erreurs plus verbeux côté serveur
});

// CORS pour autoriser les frontends (localhost:5173 = Vite, 3000 = React, 5000/5001 = ex. Kestrel)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", b => b
        .WithOrigins("https://localhost:5001", "http://localhost:5000",
            "http://localhost:5173", "http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()); // Nécessaire pour websockets + cookies/headers d'auth
});

// Injections des services applicatifs
builder.Services.AddSingleton<Matchmaker>();
builder.Services.AddSingleton<RoomService>();

// Logs console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddRazorPages(); // Optionnel: pour servir des vues de debug/doc

// 2) Configuration & Connexion DB
builder.Configuration.AddEnvironmentVariables();

// Remplacement de placeholders ${ENV_VAR} dans appsettings par les vraies valeurs d'env
var replacedConfig = ReplacePlaceholders(builder.Configuration);
var finalConnStr = replacedConfig.GetConnectionString("DefaultConnection");

// Enregistre le DbContext EF Core MySQL
builder.Services.AddDbContext<VortexDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

var app = builder.Build();

// Vérification de la connectivité DB au démarrage (log friendly)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VortexDbContext>();
    if (db.Database.CanConnect())
        app.Logger.LogInformation("✅ Connexion DB OK");
    else
        app.Logger.LogError("Impossible de se connecter à la DB");
}

// 3) Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("Dev"); // Autoriser le front à joindre /hubs/game
app.UseAuthorization();

// Expose le Hub SignalR à l'URL /hubs/game
app.MapHub<GameHub>("/hubs/game");

// (Optionnel) Razor pages
app.MapRazorPages();

// Endpoint de health-check pour la DB
app.MapGet("/health/db", async (VortexDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect
            ? Results.Ok(new { status = "UP", message = " DB reachable" })
            : Results.Problem(" DB unreachable");
    }
    catch (Exception ex)
    {
        return Results.Problem($" DB error: {ex.Message}");
    }
});

app.Run();

// --- Utils ---
// Fonction utilitaire pour remplacer les placeholders ${SOME_ENV} dans la config
// par la valeur réelle d'Environment.GetEnvironmentVariable("SOME_ENV").
static IConfiguration ReplacePlaceholders(IConfiguration config)
{
    var dict = new Dictionary<string, string?>();

    foreach (var kvp in config.AsEnumerable())
    {
        if (kvp.Value is null) continue;

        var newValue = Regex.Replace(kvp.Value, @"\$\{(.+?)\}", match =>
        {
            var envVar = match.Groups[1].Value;
            return Environment.GetEnvironmentVariable(envVar) ?? match.Value;
        });

        dict[kvp.Key] = newValue;
    }

    return new ConfigurationBuilder()
        .AddInMemoryCollection(dict)
        .Build();
}

builder.Services.AddDbContext<VortexDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions => mySqlOptions.MigrationsAssembly("DataAccess"));
});