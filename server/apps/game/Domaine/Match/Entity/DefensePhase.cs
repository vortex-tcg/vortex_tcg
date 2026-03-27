namespace game.Domaine.Match.Entity;

using System.Threading;
using game.Domaine.Match.Interface;
using game.Domaine.Match.Agregate;



public sealed class DefensePhase : IPhase
{
    public MatchPhaseType Type => MatchPhaseType.Defense;

    public void OnStartPhase(Match match, CancellationToken ct = default)
    {
        // TODO
    }

    public void OnEndPhase(Match match, CancellationToken ct = default)
    {
        // TODO
    }
}
