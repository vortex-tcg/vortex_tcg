// =============================================
// FICHIER: Program.cs (Minimal Hosting)
// Rôle: Point d'entrée de l'application ASP.NET Core.
//       Configure les services DI (SignalR, CORS, EF Core MySQL), le pipeline HTTP,
//       mappe le Hub "/hubs/game" et expose un endpoint /health/db.
// =============================================
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using game.Application.Factory;
using game.Application.Service;
using game.Infrastructure;
using game.Infrastructure.Interface;
using game.Infrastructure.Manager;
using Microsoft.AspNetCore.SignalR;


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



// Logs console
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<CreateMatchFactory>();
builder.Configuration.AddEnvironmentVariables();


var jwtSecret = builder.Configuration["JwtSettings:SecretKey"];
if (string.IsNullOrEmpty(jwtSecret))
    throw new InvalidOperationException("[JWT] JwtSettings:SecretKey est manquant ou vide dans la configuration.");

string deckApiBaseUrl = builder.Configuration["DeckApi:BaseUrl"]
                        ?? throw new InvalidOperationException("Missing DeckApi:BaseUrl in configuration");
builder.Services.AddHttpClient<IDeckApiClient, DeckApiClientManager>(client =>
{
    client.BaseAddress = new Uri(deckApiBaseUrl);
});
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
                var jwtLogger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>().CreateLogger("JWT");
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/game"))
                {
                    context.Token = accessToken;
                    jwtLogger.LogDebug("[JWT] Token reçu via query string — path={Path} tokenLength={Len}",
                        path, accessToken.ToString().Length);
                }
                else if (path.StartsWithSegments("/hubs/game") && !path.StartsWithSegments("/hubs/game/negotiate"))
                {
                    jwtLogger.LogDebug("[JWT] Connexion hub sans token query string — path={Path} ip={IP}",
                        path, context.HttpContext.Connection.RemoteIpAddress);
                }
                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                var jwtLogger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>().CreateLogger("JWT");
                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "inconnu";
                jwtLogger.LogInformation("[JWT] Token validé — userId={UserId} path={Path}",
                    userId, context.Request.Path);
                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                var jwtLogger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>().CreateLogger("JWT");
                jwtLogger.LogWarning(context.Exception,
                    "[JWT] Échec validation token — path={Path} erreur={Error}",
                    context.Request.Path, context.Exception.GetType().Name);
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                var jwtLogger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>().CreateLogger("JWT");
                jwtLogger.LogWarning("[JWT] Challenge 401 — path={Path} error={Error} description={Desc}",
                    context.Request.Path, context.Error ?? "none", context.ErrorDescription ?? "none");
                return Task.CompletedTask;
            },

            OnForbidden = context =>
            {
                var jwtLogger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>().CreateLogger("JWT");
                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "inconnu";
                jwtLogger.LogWarning("[JWT] Accès refusé 403 — userId={UserId} path={Path}",
                    userId, context.HttpContext.Request.Path);
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

app.MapHub<GameHubClean>("/hubs/game").RequireAuthorization();
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
{
    var factory = app.Services.GetRequiredService<CreateMatchFactory>();
    RoomManager.Configure(factory);
    var hubContext = app.Services.GetRequiredService<IHubContext<GameHubClean>>();
    CallManager.Configure(hubContext);
}
app.Run();
