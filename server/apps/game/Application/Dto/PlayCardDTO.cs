namespace game.Application.Dto;

using System;

public sealed class PlayCardUserDto
{
    public Guid matchId { get; init; }

    public PlayCardSelfDto self { get; init; } = new();
    public PlayCardOpponentDto opponent { get; init; } = new();
}

public sealed class PlayCardSelfDto
{
    public int gold { get; init; }
    public int position { get; init; }
    public int gameCardId { get; init; }
}

public sealed class PlayCardOpponentDto
{
    public int gold { get; init; }
    public int position { get; init; }
    public MatchInitCardDto? card { get; init; }
}