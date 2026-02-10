namespace game.Application.Dto;
public sealed class MatchFoundUserDto
{
    public Guid matchId { get; init; }
    public Guid championId { get; init; }
    public Guid opponentChampionId { get; init; }
    public int opponentHandSize { get; init; }
}