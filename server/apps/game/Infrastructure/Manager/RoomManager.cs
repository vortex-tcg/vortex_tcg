using game.Domaine.Interface;
using game.Domaine.Match.Entity;
using game.Domaine.Matchmaking.Interface;
using game.Infrastructure.Interface;

namespace game.Infrastructure.Manager;

using System;
using System.Collections.Generic;
using System.Linq;


public sealed class RoomManager :IRoomManager
{
    private readonly object _lock = new();
    private readonly List<Match> _matches = new();
    public IMatchmaker Matchmaker { get; }
    public IEventContainer MatchmakerEventContainer { get; }

    public RoomManager(IMatchmaker matchmaker, IEventContainer matchmakerEventContainer)
    {
        Matchmaker = matchmaker;
        MatchmakerEventContainer = matchmakerEventContainer;
    }

    public Match CreateMatch(List<(Guid userId, Guid deckId)> players)
    {
        // TODO: appeler la factory createMatch(players) 
        Match match = new Match
        {
            players = players
        };

        lock (_lock)
        {
            _matches.Add(match);
        }

        return match;
    }

    public Match? GetMatchByUserId(Guid userId)
    {
        lock (_lock)
        {
            return _matches.FirstOrDefault(m => m.players.Any(p => p.userId == userId));
        }
    }

    public void RemoveFinishedMatches()
    {
        lock (_lock)
        {
            _matches.RemoveAll(m => m.players.Count == 0);
        }
    }
    
}

