using System;
using System.Collections.Generic;
using System.Linq;
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

namespace VortexTCG.Tests.Auth
{
    public class AuthWebApplicationFactory : WebApplicationFactory<VortexTCG.Auth.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                Dictionary<string, string?> testSettings = new()
                {
                    ["JwtSettings:SecretKey"] = "integration-test-secret-key-123456",
                    ["CONNECTION_STRING"] = "Server=localhost;Database=vortex_test;Uid=user;Pwd=password;",
                    ["UseInMemoryDatabase"] = "true"
                };

                configBuilder.AddInMemoryCollection(testSettings);
            });

            builder.ConfigureServices(services =>
            {
                ServiceProvider sp = services.BuildServiceProvider();
                using IServiceScope scope = sp.CreateScope();
                VortexDbContext db = scope.ServiceProvider.GetRequiredService<VortexDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }

    public class ProgramTests : IClassFixture<AuthWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ProgramTests(AuthWebApplicationFactory factory)
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

        [Fact]
        public async Task Root_ReturnsServiceMetadata()
        {
            HttpResponseMessage response = await _client.GetAsync("/");
            response.EnsureSuccessStatusCode();

            RootResponse? payload = await response.Content.ReadFromJsonAsync<RootResponse>();

            Assert.NotNull(payload);
            Assert.Equal("VortexTCG.Auth", payload!.service);
            Assert.Equal("ok", payload.status);
            Assert.False(string.IsNullOrWhiteSpace(payload.message));
        }

        private sealed record HealthResponse(string status, string? message);
        private sealed record RootResponse(string message, string service, string status, DateTime utc);
    }
}
