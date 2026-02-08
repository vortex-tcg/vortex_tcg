namespace game.Application.Dto;
public sealed class MatchFoundSelfDto
{
    public Guid matchId { get; init; }
    public Guid championId { get; init; }
}

public sealed class MatchFoundOpponentDto
{
    public int opponentHandSize { get; init; }
    public Guid championId { get; init; }
}
