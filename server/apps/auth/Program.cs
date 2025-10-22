using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VortexTCG.DataAccess;
using VortexTCG.Common.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Ajout des variables d'environnement
builder.Configuration.AddEnvironmentVariables();

// JWT Authentication
var secretKey = builder.Configuration["JwtSettings:SecretKey"] 
    ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Configuration DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<VortexDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// CORS
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowVortexWeb");
app.MapControllers();

// Vérification DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VortexDbContext>();
    try
    {
        if (db.Database.CanConnect())
            app.Logger.LogInformation("Connexion DB OK");
        else
            app.Logger.LogError("Impossible de se connecter à la DB");
    }
    catch (Exception ex)
    {
        app.Logger.LogError($"Erreur DB: {ex.Message}");
    }
}

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