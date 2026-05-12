using System.Net;
using System.Net.Http.Json;
using System.Text;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.DTO;
using game.Infrastructure.Manager;

namespace game.Tests.Infrastructure.Manager;

public class DeckApiClientManagerTests
{
    private static DeckApiClientManager Make(HttpResponseMessage response)
    {
        HttpClient http = new HttpClient(new FakeHandler(response))
        {
            BaseAddress = new Uri("http://api-test/")
        };
        return new DeckApiClientManager(http);
    }

    private static ApiDeckDataDto BuildDeckData() => new ApiDeckDataDto
    {
        Champion = new ApiDeckChampionDto { ChampionID = Guid.NewGuid(), Name = "Hero", HP = 30 },
        Cards = new List<ApiDeckCardDto>()
    };

    [Fact]
    public async Task GetDeckDataAsync_ReturnsDeckData_WhenResponseIsSuccess()
    {
        ApiResultDto<ApiDeckDataDto> envelope = new ApiResultDto<ApiDeckDataDto>
        {
            success = true,
            data = BuildDeckData()
        };
        DeckApiClientManager client = Make(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(envelope)
        });

        ApiDeckDataDto result = await client.GetDeckDataAsync(new DeckId(Guid.NewGuid()));

        Assert.NotNull(result);
        Assert.Equal(30, result.Champion.HP);
    }

    [Fact]
    public async Task GetDeckDataAsync_Throws_WhenResponseBodyIsNull()
    {
        DeckApiClientManager client = Make(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetDeckDataAsync(new DeckId(Guid.NewGuid())));
    }

    [Fact]
    public async Task GetDeckDataAsync_Throws_WhenSuccessIsFalse()
    {
        ApiResultDto<ApiDeckDataDto> envelope = new ApiResultDto<ApiDeckDataDto>
        {
            success = false,
            statusCode = 404,
            message = "Deck not found"
        };
        DeckApiClientManager client = Make(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(envelope)
        });

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetDeckDataAsync(new DeckId(Guid.NewGuid())));

        Assert.Contains("404", ex.Message);
        Assert.Contains("Deck not found", ex.Message);
    }

    [Fact]
    public async Task GetDeckDataAsync_Throws_WhenDataIsNull()
    {
        ApiResultDto<ApiDeckDataDto> envelope = new ApiResultDto<ApiDeckDataDto>
        {
            success = true,
            data = null
        };
        DeckApiClientManager client = Make(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(envelope)
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.GetDeckDataAsync(new DeckId(Guid.NewGuid())));
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public FakeHandler(HttpResponseMessage response) => _response = response;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            => Task.FromResult(_response);
    }
}
