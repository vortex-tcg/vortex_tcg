using game.Domaine.Match.ValueObject;

namespace game.Domaine.Match.Entity;

using System;
using System.Collections.Generic;

public sealed class Match
{
    public MatchId matchId { get; init; } = new MatchId();
    public List<(UserId userId, DeckId deckId)> players { get; init; } = new();
}

    