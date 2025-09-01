using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql;
using System.Text;
using System.Text.RegularExpressions;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
//Chargement du .env plus précise.
Env.Load(Path.Combine(AppContext.BaseDirectory, "../../../../.env"));

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Ajout du Razor pour tester via pages
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Ajout des variables d'environnement dans la config
builder.Configuration.AddEnvironmentVariables();

// Remplacement des placeholders ${VAR} dans appsettings.json
var replacedConfig = ReplacePlaceholders(builder.Configuration);

// Construction de la chaîne finale de connexion
var finalConnStr = replacedConfig.GetConnectionString("DefaultConnection");

// Enregistrement du DbContext avec Pomelo MySQL
builder.Services.AddDbContext<VortexDbContext>(options =>
    options.UseMySql(finalConnStr, ServerVersion.AutoDetect(finalConnStr))
);

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVortexWeb",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Active CORS AVANT les endpoints
app.UseCors("AllowVortexWeb");

app.MapControllers();

// Vérification de la connexion DB au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VortexDbContext>();
    if (db.Database.CanConnect())
        app.Logger.LogInformation("✅ Connexion DB OK");
    else
        app.Logger.LogError("❌ Impossible de se connecter à la DB");
}

// Configure le pipeline HTTP
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

// Health check
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
