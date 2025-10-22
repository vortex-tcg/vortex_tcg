// =============================================
// FICHIER: Program.cs (Minimal Hosting)
// Rôle: Point d'entrée de l'application ASP.NET Core.
//       Configure les services DI (SignalR, CORS, EF Core MySQL), le pipeline HTTP,
//       mappe le Hub "/hubs/game" et expose un endpoint /health/db.
// =============================================
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.Common.Services;
using game.Hubs;
using game.Services;

var builder = WebApplication.CreateBuilder(args);

// 1) Services de base
builder.Services.AddSignalR(o => {
    o.EnableDetailedErrors = true;
});

// CORS pour autoriser les frontends
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", b => b
        .WithOrigins("https://localhost:5001", "http://localhost:5000",
            "http://localhost:5173", "http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// Injections des services applicatifs
builder.Services.AddSingleton<Matchmaker>();
builder.Services.AddSingleton<RoomService>();

// Logs console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddRazorPages();

// 2) Configuration DB - Utilise directement les variables d'environnement
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<VortexDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapControllers();

// Vérification de la connexion DB au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VortexDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        if (db.Database.CanConnect())
        {
            logger.LogInformation("Connexion DB OK");
        }
        else
        {
            logger.LogError("Impossible de se connecter à la DB (CanConnect() = false)");
            return; 
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erreur lors de la tentative de connexion à la DB");
        return; 
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("Dev");
app.UseAuthorization();

app.MapHub<GameHub>("/hubs/game");
app.MapRazorPages();

app.MapGet("/health/db", async (VortexDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect
            ? Results.Ok(new { status = "UP", message = "DB reachable" })
            : Results.Problem("DB unreachable");
    }
    catch (Exception ex)
    {
        return Results.Problem($"DB error: {ex.Message}");
    }
});

app.Run();
