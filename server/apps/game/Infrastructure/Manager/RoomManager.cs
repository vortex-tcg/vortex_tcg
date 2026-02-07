using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;
using game.Domaine.Matchmaking.Interface;
using game.Infrastructure.Interface;
using game.Application.Factory;
using game.Domaine.Interface;

namespace game.Infrastructure.Manager;

public sealed class RoomManager : IRoomManager
{
    private static readonly Lazy<RoomManager> _instance =
        new(() => new RoomManager(new Matchmaker(), null!));

    public static RoomManager Instance => _instance.Value;

    private readonly List<Match> _matches = new();
    private readonly CreateMatchFactory _createMatchFactory;

    public IMatchmaker Matchmaker { get; }
    public IEventContainer MatchmakerEventContainer { get; }

    public static void Configure(CreateMatchFactory factory)
    {
        _instance.Value._setFactory(factory);
    }

    private void _setFactory(CreateMatchFactory factory)
    {
        typeof(RoomManager)
            .GetField("_createMatchFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(this, factory);
    }

    private RoomManager(Matchmaker matchmaker, CreateMatchFactory factory)
    {
        Matchmaker = matchmaker;
        MatchmakerEventContainer = matchmaker;
        _createMatchFactory = factory;
    }

    public async Task<Match> CreateMatchAsync(List<(UserId userId, DeckId deckId)> players, CancellationToken ct = default)
    {
        if (players.Count != 2)
            throw new ArgumentException("CreateMatch requires exactly 2 players.", nameof(players));

        Match match = await _createMatchFactory.CreateMatchAsync(players[0], players[1], ct);

        lock (_matches)
        {
            _matches.Add(match);
        }

        return match;
    }

    public Match? GetMatchByUserId(UserId userId)
    {
        lock (_matches)
        {
            return _matches.FirstOrDefault(m => m.HasUser(userId));
        }
    }

    public void RemoveFinishedMatches()
    {
        lock (_matches)
        {
            _matches.RemoveAll(m => m.IsFinished);
        }
    }
}
