using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VortexTCG.DataAccess;
using Xunit;

namespace VortexTCG.Tests.Api
{
    public class ApiWebApplicationFactory : WebApplicationFactory<VortexTCG.Api.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "integration-test-secret-key-123456",
                    ["UseInMemoryDatabase"] = "true",
                    ["CONNECTION_STRING"] = "Server=localhost;Database=vortex_test;Uid=user;Pwd=password;"
                });
            });

            builder.ConfigureServices(services =>
            {
                ServiceProvider provider = services.BuildServiceProvider();
                using IServiceScope scope = provider.CreateScope();
                VortexDbContext db = scope.ServiceProvider.GetRequiredService<VortexDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }

    public class ProgramTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ProgramTests(ApiWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HealthDb_ReturnsUpStatus()
        {
            HttpResponseMessage response = await _client.GetAsync("/health/db");
            response.EnsureSuccessStatusCode();

            HealthResponse? payload = await response.Content.ReadFromJsonAsync<HealthResponse>();

            Assert.NotNull(payload);
            Assert.Equal("UP", payload!.status);
            Assert.Contains("db", payload.message ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private sealed record HealthResponse(string status, string? message);
    }
}
