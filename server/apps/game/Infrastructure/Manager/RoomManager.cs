using game.Domaine.Interface;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;
using game.Domaine.Matchmaking.Interface;
using game.Infrastructure.Interface;

namespace game.Infrastructure.Manager;

using System;
using System.Collections.Generic;
using System.Linq;


public sealed class RoomManager :IRoomManager
{
    private static readonly Lazy<RoomManager> _instance =
        new(() => new RoomManager(new Matchmaker()));

    public static RoomManager Instance => _instance.Value;

    private readonly List<Match> _matches = new();

    public IMatchmaker Matchmaker { get; }
    public IEventContainer MatchmakerEventContainer { get; }

    private RoomManager(Matchmaker matchmaker)
    {
        Matchmaker = matchmaker;
        MatchmakerEventContainer = matchmaker; 
    }

    public Match CreateMatch(List<(UserId userId, DeckId deckId)> players)
    {
        // TODO: appeler la factory createMatch(players) 
        Match match = new Match
        {
            players = players
        };

        lock (_matches)
        {
            _matches.Add(match);
        }

        return match;
    }

    public Match? GetMatchByUserId(Guid userId)
    {
        lock (_matches)
        {
            return _matches.FirstOrDefault(m => m.players.Any(p => p.userId == userId));
        }
    }

    public void RemoveFinishedMatches()
    {
        lock (_matches)
        {
            _matches.RemoveAll(m => m.players.Count == 0);
        }
    }
    
}

