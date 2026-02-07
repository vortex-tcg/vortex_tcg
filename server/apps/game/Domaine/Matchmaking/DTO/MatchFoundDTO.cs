namespace game.Domaine.Matchmaking.DTO;

public sealed class MatchFoundDto
{
    public Guid MatchId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid OpponentId { get; set; }
}
