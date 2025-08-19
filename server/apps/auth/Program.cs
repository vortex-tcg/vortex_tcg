using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using System.Text.RegularExpressions;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Common.Services;

//Chargement du .env plus précise.
Env.Load(Path.Combine(AppContext.BaseDirectory, "../../../../.env"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Ajout du Razor pour tester via pages
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Ajout des variables d'environnement dans la config pour de la sécu et de la facilité
builder.Configuration.AddEnvironmentVariables();

// Remplacement des placeholders ${VAR} dans appsettings.json car ASP.NET ne le fait pas forcément de base
var replacedConfig = ReplacePlaceholders(builder.Configuration);

// Construction de la chaîne finale de connexion
var finalConnStr = replacedConfig.GetConnectionString("DefaultConnection");

// Enregistrement du DbContext avec Pomelo MySQL
builder.Services.AddDbContext<VortexDbContext>(options =>
    options.UseMySql(
        finalConnStr,
        ServerVersion.AutoDetect(finalConnStr)
    ));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Vérification de la connexion DB au démarrage (premier health check)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VortexDbContext>();
    if (db.Database.CanConnect())
        app.Logger.LogInformation("✅ Connexion DB OK");
    else
        app.Logger.LogError("❌ Impossible de se connecter à la DB");
}

// Configure the HTTP request pipeline 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

// Ajout d'une route de vérification pour les health checks
app.MapGet("/health/db", async (VortexDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect
            ? Results.Ok(new { status = "UP", message = "✅ DB reachable" })
            : Results.Problem("❌ DB unreachable");
    }
    catch (Exception ex)
    {
        return Results.Problem($"❌ DB error: {ex.Message}");
    }
});

app.Run();


// --- Utils ---
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
