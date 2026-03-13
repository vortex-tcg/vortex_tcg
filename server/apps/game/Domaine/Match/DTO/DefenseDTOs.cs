namespace game.Domaine.Match.DTO;

public sealed class ToggleDefenseCardDto
{
    public int Position { get; set; }
    public int PositionOpponentCard { get; set; }

}
public sealed class EngagedDefenseCardDto
{
    public int Position { get; set; }
    public int GameCardId { get; set; } 
    public int PositionOpponentCard { get; set; }
}
public sealed class DefenseUpdatedDto
{ 
    public List<EngagedDefenseCardDto> EngagedCards { get; set; } = new();
}