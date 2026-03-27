using game.Domaine.Match.DTO;
using game.Domaine.Match.Interface;
using game.Domaine.Match.Service;

namespace game.Domaine.Match.Entity;

public sealed class EndTurnPhase : IPhase
{
    public MatchPhaseType Type => MatchPhaseType.EndTurn;

    public void OnStartPhase(Agregate.Match match, CancellationToken ct = default)
    {
        BattleResolveDTOs resolution = ResolveEndPhaseService.Apply(match);

        
    }

    public void OnEndPhase(Agregate.Match match, CancellationToken ct = default)
    {
        match.AttackHandler.ResetAttackHandler();
        match.DefenseHandler.ResetDefenseHandler();

    }
}