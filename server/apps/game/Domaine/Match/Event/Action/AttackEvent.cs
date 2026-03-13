namespace game.Domaine.Match.Event.Action;
using game.Domaine.Match.DTO;

public sealed record AttackOrderUpdatedData(
    Guid MatchId,
    List<EngagedCardDto> EngagedCards
);
public static class AttackEvent
{
    public const string ATTACK_ORDER_UPDATED = "ATTACK_ORDER_UPDATED";
}