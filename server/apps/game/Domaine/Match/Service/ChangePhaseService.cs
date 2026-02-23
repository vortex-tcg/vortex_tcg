using System.Threading;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.Interface;

namespace game.Domaine.Match.Service;

public static class ChangePhaseService
{
    public static void NextPhase(Agregate.Match match, CancellationToken ct = default)
    {
        IPhase actual = match.CurrentPhase;

        PhaseFlowService.NextStep step = PhaseFlowService.ComputeNext(match);
        actual.OnEndPhase(match, ct);
        match.SetCurrentPlayerPosition(step.NextCurrentPlayerPosition);
        match.SetPhase(step.NextPhase);
        step.NextPhase.OnStartPhase(match, ct);
        match.AddEvent(new DomainEvent(
            MatchEvent.PHASE_CHANGED,
            new DomainEvent.PhaseChangedData(
                match.MatchId.Value,
                match.CurrentPlayerPosition,
                step.NextPhase.Type
            )
        ));
        if (step.NextPhase.Type == Entity.MatchPhaseType.StandBy)
        {
            match.ResetTurnFlags();
        }
    }
}
