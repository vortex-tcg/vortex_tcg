using game.Domaine.Match.Entity;

namespace game.Domaine.Match.Interface;

public interface IPhase
{
    MatchPhaseType Type { get; }

    void OnStartPhase(Agregate.Match match, CancellationToken ct = default);

    void OnEndPhase(Agregate.Match match, CancellationToken ct = default);
}