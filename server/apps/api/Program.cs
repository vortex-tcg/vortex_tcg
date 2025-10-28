using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.Common.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddControllers();

var connectionString = builder.Configuration["CONNECTION_STRING"];

builder.Services.AddDbContext<VortexDbContext>(options =>
    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(11, 8, 3)) )
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
app.UseRouting();
app.UseAuthorization();

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
