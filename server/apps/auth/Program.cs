using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using System.Text.RegularExpressions;
using VortexTCG.DataAccess;
 
Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
var replacedConfig = ReplacePlaceholders(builder.Configuration);

builder.Services.AddDbContext<VortexDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(replacedConfig.GetConnectionString("DefaultConnection"))
    ));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Funtion to replace ${VAR} with true value
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
