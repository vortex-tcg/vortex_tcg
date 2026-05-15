using System.Collections;
using System.Reflection;
using game.Application.Dto;
using game.Application.Factory;
using game.Application.Service;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;
using game.Infrastructure.DTO;
using game.Infrastructure.Interface;
using game.Infrastructure.Manager;
using game.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Moq;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Domain.Application;

[Collection("ApplicationTests")]
public class QueueServiceTests : IDisposable
{
    private readonly Mock<IClientProxy> _mockProxy;

    public QueueServiceTests()
    {
        AppServiceHelpers.ClearRoom();
        AppServiceHelpers.ClearMatchmakerQueue();
        _mockProxy = AppServiceHelpers.ConfigureCallManager();
    }

    public void Dispose()
    {
        AppServiceHelpers.ClearRoom();
        AppServiceHelpers.ClearMatchmakerQueue();
    }

    private static void ConfigureMatchFactory()
    {
        Mock<IDeckApiClient> mockClient = new Mock<IDeckApiClient>();
        mockClient
            .Setup(c => c.GetDeckDataAsync(It.IsAny<DeckId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildApiDeck());
        RoomManager.Configure(new CreateMatchFactory(mockClient.Object));
    }

    private static ApiDeckDataDto BuildApiDeck() => new ApiDeckDataDto
    {
        Champion = new ApiDeckChampionDto
        {
            ChampionID = Guid.NewGuid(),
            Name = "TestHero",
            Description = "Test champion",
            HP = 30,
            Picture = "pic.png",
            FactionId = Guid.NewGuid()
        },
        Cards = new List<ApiDeckCardDto>
        {
            new ApiDeckCardDto
            {
                DeckCardId = Guid.NewGuid(),
                Quantity = 1,
                CollectionCardId = Guid.NewGuid(),
                CardId = Guid.NewGuid(),
                Name = "Card",
                Hp = 3,
                Attack = 2,
                Cost = 1,
                Description = "Test card",
                Picture = "pic.png",
                CardType = 0,
                Classes = new List<string>()
            }
        }
    };

    [Fact]
    public async Task LeaveQueueAsync_CompletesSuccessfully()
    {
        UserId userId = new UserId(Guid.NewGuid());
        DeckId deckId = new DeckId(Guid.NewGuid());
        await QueueService.JoinQueueAsync(userId, deckId);

        Exception? ex = await Record.ExceptionAsync(() => QueueService.LeaveQueueAsync(userId));

        Assert.Null(ex);
    }

    [Fact]
    public async Task JoinQueueAsync_DoesNotCallSignalR_WhenOnlyOnePlayerInQueue()
    {
        UserId userId = new UserId(Guid.NewGuid());
        DeckId deckId = new DeckId(Guid.NewGuid());

        await QueueService.JoinQueueAsync(userId, deckId);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task JoinQueueAsync_CallsSignalR_WhenTwoPlayersJoin()
    {
        ConfigureMatchFactory();

        UserId user1 = new UserId(Guid.NewGuid());
        UserId user2 = new UserId(Guid.NewGuid());
        DeckId deck1 = new DeckId(Guid.NewGuid());
        DeckId deck2 = new DeckId(Guid.NewGuid());

        await QueueService.JoinQueueAsync(user1, deck1);
        await QueueService.JoinQueueAsync(user2, deck2);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task JoinQueueAsync_SkipsNonFoundEvents_InLoop()
    {
        object matchmaker = RoomManager.Instance.Matchmaker;
        IList matchmakerEvents = (IList)matchmaker.GetType()
            .GetField("_events", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(matchmaker)!;
        matchmakerEvents.Add(new MatchmakerEvent("OTHER_EVENT", new object()));

        await QueueService.JoinQueueAsync(new UserId(Guid.NewGuid()), new DeckId(Guid.NewGuid()));

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public void InitMatch_AlwaysProducesMatchInitEvent()
    {

        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>();
        match.PullEvents();

        match.InitMatch();

        var events = match.PullEvents();
        Assert.Equal(MatchEvent.MATCH_INIT, events[0].Name);
    }

    [Fact]
    public async Task JoinQueueAsync_MapsDrawnCardsForBothPlayers()
    {
        Mock<IDeckApiClient> mockClient = new Mock<IDeckApiClient>();
        mockClient
            .Setup(c => c.GetDeckDataAsync(It.IsAny<DeckId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildApiDeckWithMultipleCards(15));
        RoomManager.Configure(new CreateMatchFactory(mockClient.Object));

        List<object[]> capturedArgs = new();
        _mockProxy
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Callback<string, object[], CancellationToken>((_, args, _) => capturedArgs.Add(args))
            .Returns(Task.CompletedTask);

        await QueueService.JoinQueueAsync(new UserId(Guid.NewGuid()), new DeckId(Guid.NewGuid()));
        await QueueService.JoinQueueAsync(new UserId(Guid.NewGuid()), new DeckId(Guid.NewGuid()));

        List<MatchInitUserDto> dtos = capturedArgs
            .Where(a => a.Length > 0 && a[0] is MatchInitUserDto)
            .Select(a => (MatchInitUserDto)a[0])
            .ToList();

        Assert.Equal(2, dtos.Count);
        Assert.All(dtos, dto => Assert.NotEmpty(dto.self.drawnCards));
    }

    private static ApiDeckDataDto BuildApiDeckWithMultipleCards(int count)
    {
        List<ApiDeckCardDto> cards = new();
        for (int i = 0; i < count; i++)
        {
            cards.Add(new ApiDeckCardDto
            {
                DeckCardId = Guid.NewGuid(),
                Quantity = 1,
                CollectionCardId = Guid.NewGuid(),
                CardId = Guid.NewGuid(),
                Name = $"Card{i}",
                Hp = 3,
                Attack = 2,
                Cost = 1,
                Description = "Test",
                Picture = "pic.png",
                CardType = 0,
                Classes = new List<string>()
            });
        }
        return new ApiDeckDataDto
        {
            Champion = new ApiDeckChampionDto
            {
                ChampionID = Guid.NewGuid(),
                Name = "TestHero",
                Description = "desc",
                HP = 30,
                Picture = "pic.png",
                FactionId = Guid.NewGuid()
            },
            Cards = cards
        };
    }
}
