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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

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
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<RoomService>();

// Logs console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddRazorPages();

var jwtSecret = builder.Configuration["JwtSettings:SecretKey"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            NameClaimType = ClaimTypes.NameIdentifier,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            RequireExpirationTime = false,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/game")) {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    }
);

builder.Services.AddAuthorization();

// 2) Configuration DB - Utilise directement les variables d'environnement
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration["CONNECTION_STRING"];

builder.Services.AddDbContext<VortexDbContext>(options =>
    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(11, 8, 3)))
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add services to the container
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
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<GameHub>("/hubs/game").RequireAuthorization();
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
