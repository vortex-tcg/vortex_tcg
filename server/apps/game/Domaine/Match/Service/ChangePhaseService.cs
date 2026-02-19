using game.Domaine.Match.Interface;

namespace game.Domaine.Match.Service;

using System.Threading;


public static class ChangePhaseService
{
    public static void ChangePhase(
        Agregate.Match match,
        IPhase actualPhase,
        IPhase nextPhase,
        CancellationToken ct = default
    )
    {
        actualPhase.OnEndPhase(match, ct);
        nextPhase.OnStartPhase(match, ct);
    }
}
