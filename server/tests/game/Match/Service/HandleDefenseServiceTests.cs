using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Domaine.Match.DTO;
using game.Tests.Helpers;
using MatchAggregate = game.Domaine.Match.Agregate.Match;
using VCardState = game.Domaine.Match.ValueObject.CardState;

namespace game.Tests.Domain.Service;

public class HandleDefenseServiceTests
{
    private static (MatchAggregate match, UserId defenderId) BuildDefensePhaseMatch(
        int defenderCardId = 10,
        int attackerCardId = 20,
        int attackPosition = 1,
        int defensePosition = 2)
    {
        UserId attackerId = new UserId(Guid.NewGuid());
        UserId defenderId = new UserId(Guid.NewGuid());

        GameCardDto attackCard = MatchHelpers.MakeCard(attackerCardId, state: VCardState.Attacking);
        GameCardDto defenseCard = MatchHelpers.MakeCard(defenderCardId, state: VCardState.Active);

        Player attacker = MatchHelpers.MakePlayer(userId: attackerId);
        attacker.Board.Place(attackPosition, attackCard);

        Player defender = MatchHelpers.MakePlayer(userId: defenderId);
        defender.Board.Place(defensePosition, defenseCard);

        MatchAggregate match = new MatchAggregate(attacker, defender, new DefensePhase());
        match.SetCurrentPlayerPosition(2);
        match.AttackHandler.AddAttack(attackPosition, attackerCardId);

        return (match, defenderId);
    }

    [Fact]
    public void ToggleDefenseCard_EngagesCard_WhenActive()
    {
        (MatchAggregate match, UserId defenderId) = BuildDefensePhaseMatch();

        DefenseUpdatedDto dto = HandleDefenseService.ToggleDefenseCard(match, defenderId, defensePosition: 2, attackPosition: 1);

        Assert.Single(dto.EngagedCards);
        Assert.Equal(2, dto.EngagedCards[0].Position);
        Assert.Equal(1, dto.EngagedCards[0].PositionOpponentCard);

        GameCardDto? card = match.Player2.Board.GetCardAtPosition(2);
        Assert.Equal(CardStates.Defending.Value, card!.States.Value);
    }

    [Fact]
    public void ToggleDefenseCard_Throws_WhenClickingSamePositiveAttackOnDefendingCard()
    {
        // A defending card (States=Defending) cannot re-engage via positive attackPosition.
        // Removal requires attackPosition < 0 (explicit removal flow).
        (MatchAggregate match, UserId defenderId) = BuildDefensePhaseMatch();
        HandleDefenseService.ToggleDefenseCard(match, defenderId, defensePosition: 2, attackPosition: 1);

        Assert.Throws<InvalidOperationException>(() =>
            HandleDefenseService.ToggleDefenseCard(match, defenderId, defensePosition: 2, attackPosition: 1));
    }

    [Fact]
    public void ToggleDefenseCard_RemovesCard_WhenAttackPositionIsNegative()
    {
        (MatchAggregate match, UserId defenderId) = BuildDefensePhaseMatch();
        HandleDefenseService.ToggleDefenseCard(match, defenderId, defensePosition: 2, attackPosition: 1);

        DefenseUpdatedDto dto = HandleDefenseService.ToggleDefenseCard(match, defenderId, defensePosition: 2, attackPosition: -1);

        Assert.Empty(dto.EngagedCards);

        GameCardDto? card = match.Player2.Board.GetCardAtPosition(2);
        Assert.Equal(CardStates.Active.Value, card!.States.Value);
    }

    [Fact]
    public void ToggleDefenseCard_Throws_WhenAnotherCardAlreadyDefendsThisAttack()
    {
        UserId attackerId = new UserId(Guid.NewGuid());
        UserId defenderId = new UserId(Guid.NewGuid());

        GameCardDto attackCard = MatchHelpers.MakeCard(20, state: VCardState.Attacking);
        GameCardDto defCard1 = MatchHelpers.MakeCard(10, state: VCardState.Active);
        GameCardDto defCard2 = MatchHelpers.MakeCard(11, state: VCardState.Active);

        Player attacker = MatchHelpers.MakePlayer(userId: attackerId);
        attacker.Board.Place(1, attackCard);

        Player defender = MatchHelpers.MakePlayer(userId: defenderId);
        defender.Board.Place(2, defCard1);
        defender.Board.Place(3, defCard2);

        MatchAggregate match = new MatchAggregate(attacker, defender, new DefensePhase());
        match.SetCurrentPlayerPosition(2);
        match.AttackHandler.AddAttack(1, 20);

        HandleDefenseService.ToggleDefenseCard(match, defenderId, defensePosition: 2, attackPosition: 1);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            HandleDefenseService.ToggleDefenseCard(match, defenderId, defensePosition: 3, attackPosition: 1));

        Assert.Contains("autre carte", ex.Message);
    }

    [Fact]
    public void ToggleDefenseCard_DisengagesCard_WhenAlreadyDefendingThisAttack()
    {
        (MatchAggregate match, UserId defenderId) = BuildDefensePhaseMatch();
        GameCardDto? defCard = match.Player2.Board.GetCardAtPosition(2);
        match.DefenseHandler.AddOrReplaceDefense(2, defCard!.GameCardId, 1);
        DefenseUpdatedDto dto = HandleDefenseService.ToggleDefenseCard(match, defenderId, defensePosition: 2, attackPosition: 1);
        Assert.Empty(dto.EngagedCards);
        Assert.Equal(CardStates.Active.Value, defCard.States.Value);
    }

    [Fact]
    public void ToggleDefenseCard_DoesNothing_WhenNotDefensePhase()
    {
        UserId defenderId = new UserId(Guid.NewGuid());
        GameCardDto defCard = MatchHelpers.MakeCard(10, state: VCardState.Active);

        Player defender = MatchHelpers.MakePlayer(userId: defenderId);
        defender.Board.Place(2, defCard);

        MatchAggregate match = new MatchAggregate(MatchHelpers.MakePlayer(), defender, new StandByPhase());
        match.SetCurrentPlayerPosition(2);

        DefenseUpdatedDto dto = HandleDefenseService.ToggleDefenseCard(match, defenderId, defensePosition: 2, attackPosition: 1);

        Assert.Empty(dto.EngagedCards);
    }

    [Fact]
    public void ToggleDefenseCard_Throws_WhenCardIsSleeping()
    {
        UserId attackerId = new UserId(Guid.NewGuid());
        UserId defenderId = new UserId(Guid.NewGuid());

        GameCardDto attackCard = MatchHelpers.MakeCard(20, state: VCardState.Attacking);
        GameCardDto sleepingCard = MatchHelpers.MakeCard(10, state: VCardState.Sleeping);

        Player attacker = MatchHelpers.MakePlayer(userId: attackerId);
        attacker.Board.Place(1, attackCard);

        Player defender = MatchHelpers.MakePlayer(userId: defenderId);
        defender.Board.Place(2, sleepingCard);

        MatchAggregate match = new MatchAggregate(attacker, defender, new DefensePhase());
        match.SetCurrentPlayerPosition(2);

        Assert.Throws<InvalidOperationException>(() =>
            HandleDefenseService.ToggleDefenseCard(match, defenderId, defensePosition: 2, attackPosition: 1));
    }
}
