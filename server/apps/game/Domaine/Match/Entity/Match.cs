namespace game.Domaine.Match.Entity;

using System;
using System.Collections.Generic;

public sealed class Match
{
    public Guid matchId { get; init; } = Guid.NewGuid();
    public List<(Guid userId, Guid deckId)> players { get; init; } = new();
}

