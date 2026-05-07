using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;
using MatchAggregate = game.Domaine.Match.Agregate.Match;
using VCardState = game.Domaine.Match.ValueObject.CardState;

namespace game.Tests.Domain.Service;

public class PhaseFlowServiceTests
{
    [Fact]
    public void ComputeNext_FromStandBy_GoesToAttack_WhenActiveCardOnBoard()
    {
        GameCardDto card = MatchHelpers.MakeCard(1, state: VCardState.Active);
        Player p1 = MatchHelpers.MakePlayer();
        p1.Board.Place(1, card);

        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>(p1: p1);

        PhaseFlowService.NextStep step = PhaseFlowService.ComputeNext(match);

        Assert.Equal(MatchPhaseType.Attack, step.NextPhase.Type);
        Assert.Equal(1, step.NextCurrentPlayerPosition);
    }

    [Fact]
    public void ComputeNext_FromStandBy_GoesToEndTurn_WhenNoActiveCardOnBoard()
    {
        GameCardDto card = MatchHelpers.MakeCard(1, state: VCardState.Sleeping);
        Player p1 = MatchHelpers.MakePlayer();
        p1.Board.Place(1, card);

        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>(p1: p1);

        PhaseFlowService.NextStep step = PhaseFlowService.ComputeNext(match);

        Assert.Equal(MatchPhaseType.EndTurn, step.NextPhase.Type);
        Assert.Equal(2, step.NextCurrentPlayerPosition);
    }

    [Fact]
    public void ComputeNext_FromStandBy_GoesToEndTurn_WhenBoardIsEmpty()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>();

        PhaseFlowService.NextStep step = PhaseFlowService.ComputeNext(match);

        Assert.Equal(MatchPhaseType.EndTurn, step.NextPhase.Type);
    }

    [Fact]
    public void ComputeNext_FromAttack_GoesToDefense_WhenPendingDefense()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<AttackPhase>();
        match.SetPendingDefense(true);

        PhaseFlowService.NextStep step = PhaseFlowService.ComputeNext(match);

        Assert.Equal(MatchPhaseType.Defense, step.NextPhase.Type);
        Assert.Equal(2, step.NextCurrentPlayerPosition);
    }

    [Fact]
    public void ComputeNext_FromAttack_GoesToEndTurn_WhenNoPendingDefense()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<AttackPhase>();

        PhaseFlowService.NextStep step = PhaseFlowService.ComputeNext(match);

        Assert.Equal(MatchPhaseType.EndTurn, step.NextPhase.Type);
        Assert.Equal(2, step.NextCurrentPlayerPosition);
    }

    [Fact]
    public void ComputeNext_FromDefense_GoesToEndTurn_KeepingCurrentPlayer()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<DefensePhase>();
        match.SetCurrentPlayerPosition(2);

        PhaseFlowService.NextStep step = PhaseFlowService.ComputeNext(match);

        Assert.Equal(MatchPhaseType.EndTurn, step.NextPhase.Type);
        Assert.Equal(2, step.NextCurrentPlayerPosition);
    }

    [Fact]
    public void ComputeNext_FromEndTurn_GoesToStandBy_KeepingCurrentPlayer()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<EndTurnPhase>();
        match.SetCurrentPlayerPosition(2);

        PhaseFlowService.NextStep step = PhaseFlowService.ComputeNext(match);

        Assert.Equal(MatchPhaseType.StandBy, step.NextPhase.Type);
        Assert.Equal(2, step.NextCurrentPlayerPosition);
    }
}
