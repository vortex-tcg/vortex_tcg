namespace game.Domaine.Match.DTO;
public sealed class ToggleAttackCardDto
{
    public int Position { get; set; }
}
public sealed class EngagedCardDto
{
    public int Position { get; set; }
    public int GameCardId { get; set; } 
    public int AttackOrder { get; set; }
}
public sealed class AttackOrderUpdatedDto
{ 
    public List<EngagedCardDto> EngagedCards { get; set; } = new();
}