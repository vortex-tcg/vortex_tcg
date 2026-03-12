using game.Domaine.Match.Interface;

namespace game.Domaine.Match.Entity;

public sealed class AttackPhase : IPhase
{
    public MatchPhaseType Type => MatchPhaseType.Attack;

    public void OnStartPhase(Agregate.Match match, CancellationToken ct = default)
    {
        match.AttackHandler.ResetAttackHandler();
    }

    public void OnEndPhase(Agregate.Match match, CancellationToken ct = default)
    {
        // TODO
    }
}