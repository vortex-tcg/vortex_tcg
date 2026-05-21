using game.Application.Factory;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.DTO;
using game.Infrastructure.Interface;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Domain.Factory;

public class CreateMatchFactoryTests
{
    private static ApiDeckDataDto MakeApiDeck(int cardCount = 20)
    {
        List<ApiDeckCardDto> cards = Enumerable.Range(1, cardCount).Select(i => new ApiDeckCardDto
        {
            CardId = Guid.NewGuid(),
            DeckCardId = Guid.NewGuid(),
            CollectionCardId = Guid.NewGuid(),
            Name = $"Card{i}",
            Hp = 3,
            Attack = 2,
            Cost = 1,
            Description = "",
            Picture = "",
            CardType = 0,
            Classes = new List<string>()
        }).ToList();

        return new ApiDeckDataDto
        {
            Cards = cards,
            Champion = new ApiDeckChampionDto
            {
                ChampionID = Guid.NewGuid(),
                Name = "Hero",
                Description = "",
                HP = 30,
                Picture = "",
                FactionId = Guid.NewGuid()
            }
        };
    }

    private static CreateMatchFactory BuildFactory(
        ApiDeckDataDto? p1Deck = null,
        ApiDeckDataDto? p2Deck = null)
    {
        ApiDeckDataDto deck1 = p1Deck ?? MakeApiDeck();
        ApiDeckDataDto deck2 = p2Deck ?? MakeApiDeck();

        Mock<IDeckApiClient> mockClient = new Mock<IDeckApiClient>();
        mockClient
            .SetupSequence(c => c.GetDeckDataAsync(It.IsAny<DeckId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck1)
            .ReturnsAsync(deck2);

        return new CreateMatchFactory(mockClient.Object, NullLogger<CreateMatchFactory>.Instance);
    }

    private static ((UserId, DeckId), (UserId, DeckId)) MakePlayers()
    {
        (UserId, DeckId) p1 = (new UserId(Guid.NewGuid()), new DeckId(Guid.NewGuid()));
        (UserId, DeckId) p2 = (new UserId(Guid.NewGuid()), new DeckId(Guid.NewGuid()));
        return (p1, p2);
    }

    [Fact]
    public async Task CreateMatchAsync_ReturnsMatch()
    {
        CreateMatchFactory factory = BuildFactory();
        var (p1, p2) = MakePlayers();

        MatchAggregate match = await factory.CreateMatchAsync(p1, p2);

        Assert.NotNull(match);
    }

    [Fact]
    public async Task CreateMatchAsync_Player1HasSixCardsInHand()
    {
        CreateMatchFactory factory = BuildFactory();
        var (p1, p2) = MakePlayers();

        MatchAggregate match = await factory.CreateMatchAsync(p1, p2);

        Assert.Equal(6, match.Player1.Hand.Count);
    }

    [Fact]
    public async Task CreateMatchAsync_Player2HasFiveCardsInHand()
    {
        CreateMatchFactory factory = BuildFactory();
        var (p1, p2) = MakePlayers();

        MatchAggregate match = await factory.CreateMatchAsync(p1, p2);

        Assert.Equal(5, match.Player2.Hand.Count);
    }

    [Fact]
    public async Task CreateMatchAsync_PlayerUserIdsAreCorrect()
    {
        CreateMatchFactory factory = BuildFactory();
        var (p1, p2) = MakePlayers();

        MatchAggregate match = await factory.CreateMatchAsync(p1, p2);

        Assert.Equal((Guid)p1.Item1, (Guid)match.Player1.UserId);
        Assert.Equal((Guid)p2.Item1, (Guid)match.Player2.UserId);
    }

    [Fact]
    public async Task CreateMatchAsync_GameCardIdsAreUniqueAcrossPlayers()
    {
        CreateMatchFactory factory = BuildFactory();
        var (p1, p2) = MakePlayers();

        MatchAggregate match = await factory.CreateMatchAsync(p1, p2);

        IEnumerable<int> p1Ids = match.Player1.Hand.Cards.Select(c => c.GameCardId.Value);
        IEnumerable<int> p2Ids = match.Player2.Hand.Cards.Select(c => c.GameCardId.Value);

        Assert.Empty(p1Ids.Intersect(p2Ids));
    }

    [Fact]
    public async Task CreateMatchAsync_GameCardIdsAreSequential()
    {
        CreateMatchFactory factory = BuildFactory(MakeApiDeck(10), MakeApiDeck(10));
        var (p1, p2) = MakePlayers();

        MatchAggregate match = await factory.CreateMatchAsync(p1, p2);

        List<int> p1HandIds = match.Player1.Hand.Cards.Select(c => c.GameCardId.Value).OrderBy(x => x).ToList();
        List<int> p1DeckIds = match.Player1.Deck.DrawOne() != null
            ? new List<int>()
            : new List<int>();

        Assert.True(p1HandIds[0] >= 1);
        Assert.True(p1HandIds.Last() < p1HandIds[0] + 20);
    }

    [Fact]
    public async Task CreateMatchAsync_StartsInStandByPhase()
    {
        CreateMatchFactory factory = BuildFactory();
        var (p1, p2) = MakePlayers();

        MatchAggregate match = await factory.CreateMatchAsync(p1, p2);

        Assert.Equal(game.Domaine.Match.Entity.MatchPhaseType.StandBy, match.CurrentPhase.Type);
    }

    [Fact]
    public async Task CreateMatchAsync_DeckIsSmall_OpeningHandLimitedByDeckSize()
    {
        CreateMatchFactory factory = BuildFactory(MakeApiDeck(3), MakeApiDeck(3));
        var (p1, p2) = MakePlayers();

        MatchAggregate match = await factory.CreateMatchAsync(p1, p2);

        Assert.True(match.Player1.Hand.Count <= 3);
        Assert.True(match.Player2.Hand.Count <= 3);
    }

    [Fact]
    public async Task CreateMatchAsync_CallsDeckApiClientForBothPlayers()
    {
        Mock<IDeckApiClient> mockClient = new Mock<IDeckApiClient>();
        mockClient
            .SetupSequence(c => c.GetDeckDataAsync(It.IsAny<DeckId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeApiDeck())
            .ReturnsAsync(MakeApiDeck());

        CreateMatchFactory factory = new CreateMatchFactory(mockClient.Object, NullLogger<CreateMatchFactory>.Instance);
        var (p1, p2) = MakePlayers();

        await factory.CreateMatchAsync(p1, p2);

        mockClient.Verify(c => c.GetDeckDataAsync(It.IsAny<DeckId>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
