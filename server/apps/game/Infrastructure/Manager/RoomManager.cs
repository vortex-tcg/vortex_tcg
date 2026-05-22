using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;
using game.Domaine.Matchmaking.Interface;
using game.Infrastructure.Interface;
using game.Application.Factory;
using game.Domaine.Interface;
using game.Domaine.Match.Agregate;
using Microsoft.Extensions.Logging;

namespace game.Infrastructure.Manager;

public sealed class RoomManager : IRoomManager
{
    private static readonly Lazy<RoomManager> _instance =
        new(() => new RoomManager(new Matchmaker(), null!));

    public static RoomManager Instance => _instance.Value;

    private readonly List<Match> _matches = new();
    private readonly CreateMatchFactory _createMatchFactory;
    private ILogger? _logger;

    public IMatchmaker Matchmaker { get; }
    public IEventContainer MatchmakerEventContainer { get; }

    public static void Configure(CreateMatchFactory factory)
    {
        _instance.Value._setFactory(factory);
    }

    public static void SetLogger(ILoggerFactory loggerFactory)
    {
        _instance.Value._logger = loggerFactory.CreateLogger<RoomManager>();
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

        _logger?.LogInformation("[ROOM] CreateMatchAsync — {P1} vs {P2}", players[0].userId, players[1].userId);

        Match match = await _createMatchFactory.CreateMatchAsync(players[0], players[1], ct);

        lock (_matches)
        {
            _matches.Add(match);
            _logger?.LogDebug("[ROOM] Match ajouté — matchId={MatchId} | matches actifs: {MatchCount}", match.MatchId, _matches.Count);
        }

        return match;
    }

    public void RemoveMatch(Match match)
    {
        if (match == null) return;

        lock (_matches)
        {
            bool removed = _matches.Remove(match);
            _logger?.LogInformation("[ROOM] RemoveMatch — matchId={MatchId} retiré={Removed} | matches actifs: {MatchCount}", match.MatchId, removed, _matches.Count);
        }
    }

    public Match? GetMatchByUserId(UserId userId)
    {
        lock (_matches)
        {
            Match? match = _matches.FirstOrDefault(m => m.HasUser(userId));
            if (match == null)
                _logger?.LogDebug("[ROOM] GetMatchByUserId — aucun match trouvé pour userId={UserId}", userId);
            return match;
        }
    }

    public void RemoveFinishedMatches()
    {
        lock (_matches)
        {
            int before = _matches.Count;
            _matches.RemoveAll(m => m.IsFinished);
            int removed = before - _matches.Count;
            if (removed > 0)
                _logger?.LogInformation("[ROOM] RemoveFinishedMatches — {Removed} match(es) supprimé(s) | restants: {MatchCount}", removed, _matches.Count);
        }
    }
}
