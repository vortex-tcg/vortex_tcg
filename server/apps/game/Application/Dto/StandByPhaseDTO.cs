namespace game.Application.Dto;

using System; 

public sealed class StandByDto
{
    public Guid matchId { get; set; }
    public Guid currentPlayerUserId { get; set; }

    public int playerGold { get; set; }
    public int opponentGold { get; set; }

    public int playerHandCount { get; set; }
    public int opponentHandCount { get; set; }

    public MatchInitCardDto? drawnCard { get; set; } 
}
