using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Domaine.Match.DTO;
using game.Tests.Helpers;
using MatchAggregate = game.Domaine.Match.Agregate.Match;
using VCardState = game.Domaine.Match.ValueObject.CardState;

namespace game.Tests.Domain.Service;

public class HandleAttackServiceTests
{
    private static (MatchAggregate match, UserId p1Id) BuildAttackPhaseMatch()
    {
        UserId p1Id = new UserId(Guid.NewGuid());
        GameCardDto card = MatchHelpers.MakeCard(1, state: VCardState.Active);

        Player p1 = MatchHelpers.MakePlayer(userId: p1Id);
        p1.Board.Place(1, card);

        MatchAggregate match = MatchHelpers.MakeMatchInPhase<AttackPhase>(p1: p1);
        return (match, p1Id);
    }

    [Fact]
    public void ToggleAttackCard_EngagesCard_WhenActive()
    {
        (MatchAggregate match, UserId p1Id) = BuildAttackPhaseMatch();

        AttackOrderUpdatedDto dto = HandleAttackService.ToggleAttackCard(match, p1Id, 1);

        Assert.Single(dto.EngagedCards);
        Assert.Equal(1, dto.EngagedCards[0].Position);

        GameCardDto? card = match.Player1.Board.GetCardAtPosition(1);
        Assert.Equal(CardStates.Attacking.Value, card!.States.Value);
    }

    [Fact]
    public void ToggleAttackCard_DisengagesCard_WhenAlreadyEngaged()
    {
        (MatchAggregate match, UserId p1Id) = BuildAttackPhaseMatch();

        HandleAttackService.ToggleAttackCard(match, p1Id, 1);
        AttackOrderUpdatedDto dto = HandleAttackService.ToggleAttackCard(match, p1Id, 1);

        Assert.Empty(dto.EngagedCards);

        GameCardDto? card = match.Player1.Board.GetCardAtPosition(1);
        Assert.Equal(CardStates.Active.Value, card!.States.Value);
    }

    [Fact]
    public void ToggleAttackCard_DoesNothing_WhenNotAttackPhase()
    {
        UserId p1Id = new UserId(Guid.NewGuid());
        GameCardDto card = MatchHelpers.MakeCard(1, state: VCardState.Active);

        Player p1 = MatchHelpers.MakePlayer(userId: p1Id);
        p1.Board.Place(1, card);

        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>(p1: p1);

        AttackOrderUpdatedDto dto = HandleAttackService.ToggleAttackCard(match, p1Id, 1);

        Assert.Empty(dto.EngagedCards);
        Assert.Equal(CardStates.Active.Value, card.States.Value);
    }

    [Fact]
    public void ToggleAttackCard_DoesNothing_WhenNotCurrentPlayer()
    {
        (MatchAggregate match, _) = BuildAttackPhaseMatch();
        UserId wrongUser = new UserId(Guid.NewGuid());

        AttackOrderUpdatedDto dto = HandleAttackService.ToggleAttackCard(match, wrongUser, 1);

        Assert.Empty(dto.EngagedCards);
    }

    [Fact]
    public void ToggleAttackCard_DoesNothing_WhenCardNotOnBoard()
    {
        (MatchAggregate match, UserId p1Id) = BuildAttackPhaseMatch();

        AttackOrderUpdatedDto dto = HandleAttackService.ToggleAttackCard(match, p1Id, 99);

        Assert.Empty(dto.EngagedCards);
    }

    [Fact]
    public void ToggleAttackCard_DoesNotEngage_WhenCardIsSleeping()
    {
        UserId p1Id = new UserId(Guid.NewGuid());
        GameCardDto card = MatchHelpers.MakeCard(1, state: VCardState.Sleeping);

        Player p1 = MatchHelpers.MakePlayer(userId: p1Id);
        p1.Board.Place(1, card);

        MatchAggregate match = MatchHelpers.MakeMatchInPhase<AttackPhase>(p1: p1);

        AttackOrderUpdatedDto dto = HandleAttackService.ToggleAttackCard(match, p1Id, 1);

        Assert.Empty(dto.EngagedCards);
    }

    [Fact]
    public void ToggleAttackCard_SetsPendingDefense_WhenCardEngaged()
    {
        (MatchAggregate match, UserId p1Id) = BuildAttackPhaseMatch();

        HandleAttackService.ToggleAttackCard(match, p1Id, 1);

        Assert.True(match.HasPendingDefense);
    }

    [Fact]
    public void ToggleAttackCard_AddsEventToMatch()
    {
        (MatchAggregate match, UserId p1Id) = BuildAttackPhaseMatch();
        match.PullEvents();

        HandleAttackService.ToggleAttackCard(match, p1Id, 1);

        IReadOnlyList<game.Domaine.Interface.IEvent> events = match.PullEvents();
        Assert.Single(events);
    }
}
