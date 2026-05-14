using game.Domaine.Interface;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;

namespace game.Tests.Matchmaking.Agregate;

public class MatchmakerTests
{
    private static Matchmaker Make() => new Matchmaker();

    // ── JoinQueueAsync ───────────────────────────────────────────

    [Fact]
    public async Task JoinQueueAsync_DoesNotEmitEvent_WhenOnlyOnePlayerJoins()
    {
        Matchmaker mm = Make();
        UserId userId = new UserId(Guid.NewGuid());
        DeckId deckId = new DeckId(Guid.NewGuid());

        await mm.JoinQueueAsync(userId, deckId);

        IReadOnlyList<IEvent> events = mm.PullEvents();
        Assert.Empty(events);
    }

    [Fact]
    public async Task JoinQueueAsync_EmitsFoundEvent_WhenTwoPlayersJoin()
    {
        Matchmaker mm = Make();
        UserId user1 = new UserId(Guid.NewGuid());
        UserId user2 = new UserId(Guid.NewGuid());

        await mm.JoinQueueAsync(user1, new DeckId(Guid.NewGuid()));
        await mm.JoinQueueAsync(user2, new DeckId(Guid.NewGuid()));

        IReadOnlyList<IEvent> events = mm.PullEvents();
        Assert.Single(events);
        Assert.Equal(MatchmakerEvent.FOUND, events[0].Name);
    }

    [Fact]
    public async Task JoinQueueAsync_FoundDataContainsBothPlayers()
    {
        Matchmaker mm = Make();
        UserId user1 = new UserId(Guid.NewGuid());
        UserId user2 = new UserId(Guid.NewGuid());
        DeckId deck1 = new DeckId(Guid.NewGuid());
        DeckId deck2 = new DeckId(Guid.NewGuid());

        await mm.JoinQueueAsync(user1, deck1);
        await mm.JoinQueueAsync(user2, deck2);

        MatchFoundData data = mm.PullEvents()[0].GetData<MatchFoundData>();
        Assert.Equal(2, data.players.Count);
        Assert.Contains(data.players, p => p.userId.Equals(user1));
        Assert.Contains(data.players, p => p.userId.Equals(user2));
    }

    [Fact]
    public async Task JoinQueueAsync_RemovesBothPlayersFromQueue_AfterMatch()
    {
        Matchmaker mm = Make();
        UserId user1 = new UserId(Guid.NewGuid());
        UserId user2 = new UserId(Guid.NewGuid());
        UserId user3 = new UserId(Guid.NewGuid());

        await mm.JoinQueueAsync(user1, new DeckId(Guid.NewGuid()));
        await mm.JoinQueueAsync(user2, new DeckId(Guid.NewGuid()));
        mm.PullEvents(); // consomme l'event FOUND

        // User3 rejoint — aucun des deux précédents ne devrait matcher (queue vide)
        await mm.JoinQueueAsync(user3, new DeckId(Guid.NewGuid()));

        IReadOnlyList<IEvent> events = mm.PullEvents();
        Assert.Empty(events);
    }

    [Fact]
    public async Task JoinQueueAsync_OverwritesDeckId_WhenSameUserJoinsTwice()
    {
        Matchmaker mm = Make();
        UserId user1 = new UserId(Guid.NewGuid());
        UserId user2 = new UserId(Guid.NewGuid());
        DeckId firstDeck = new DeckId(Guid.NewGuid());
        DeckId secondDeck = new DeckId(Guid.NewGuid());

        await mm.JoinQueueAsync(user1, firstDeck);
        await mm.JoinQueueAsync(user1, secondDeck); // même user, deck différent

        // Rejoindre avec user2 doit déclencher un match avec le second deck de user1
        await mm.JoinQueueAsync(user2, new DeckId(Guid.NewGuid()));

        MatchFoundData data = mm.PullEvents()[0].GetData<MatchFoundData>();
        var user1Entry = data.players.First(p => p.userId.Equals(user1));
        Assert.Equal(secondDeck, user1Entry.deckId);
    }

    // ── LeaveQueueAsync ──────────────────────────────────────────

    [Fact]
    public async Task LeaveQueueAsync_RemovesPlayer_SoNoMatchIsFound()
    {
        Matchmaker mm = Make();
        UserId user1 = new UserId(Guid.NewGuid());
        UserId user2 = new UserId(Guid.NewGuid());

        await mm.JoinQueueAsync(user1, new DeckId(Guid.NewGuid()));
        await mm.LeaveQueueAsync(user1);

        // user2 rejoint seul — pas de match
        await mm.JoinQueueAsync(user2, new DeckId(Guid.NewGuid()));

        IReadOnlyList<IEvent> events = mm.PullEvents();
        Assert.Empty(events);
    }

    [Fact]
    public async Task LeaveQueueAsync_DoesNotThrow_WhenPlayerNotInQueue()
    {
        Matchmaker mm = Make();
        UserId userId = new UserId(Guid.NewGuid());

        Exception? ex = await Record.ExceptionAsync(() => mm.LeaveQueueAsync(userId));

        Assert.Null(ex);
    }

    // ── PullEvents ───────────────────────────────────────────────

    [Fact]
    public void PullEvents_ReturnsEmpty_WhenNoEventsExist()
    {
        Matchmaker mm = Make();

        IReadOnlyList<IEvent> events = mm.PullEvents();

        Assert.Empty(events);
    }

    [Fact]
    public async Task PullEvents_ClearsEvents_AfterFirstCall()
    {
        Matchmaker mm = Make();
        await mm.JoinQueueAsync(new UserId(Guid.NewGuid()), new DeckId(Guid.NewGuid()));
        await mm.JoinQueueAsync(new UserId(Guid.NewGuid()), new DeckId(Guid.NewGuid()));

        IReadOnlyList<IEvent> first = mm.PullEvents();
        IReadOnlyList<IEvent> second = mm.PullEvents();

        Assert.Single(first);
        Assert.Empty(second);
    }
}