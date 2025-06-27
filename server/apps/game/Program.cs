using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using System.Text.RegularExpressions;
using VortexTCG.DataAccess;

Env.Load("../../.env");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Configuration.AddEnvironmentVariables();
var replacedConfig = ReplacePlaceholders(builder.Configuration);

builder.Services.AddDbContext<VortexDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(replacedConfig.GetConnectionString("DefaultConnection"))
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

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