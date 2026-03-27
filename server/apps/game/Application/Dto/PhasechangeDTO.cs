namespace game.Application.Dto;


using System;
using game.Domaine.Match.Entity;

public sealed class PhaseChangedDto
{
    public Guid matchId { get; init; }
    public int currentPlayerPosition { get; init; }
    public MatchPhaseType phase { get; init; }
}
