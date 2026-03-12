using game.Domaine.Match.ValueObject;

namespace game.Domaine.Match.Service;

using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Interface;
public static class PhaseFlowService
{
    public sealed record NextStep(IPhase NextPhase, int NextCurrentPlayerPosition);

    public static NextStep ComputeNext(Match match)
    {
        int curPos = match.CurrentPlayerPosition;

        return match.CurrentPhase.Type switch
        {
            // StandBy:
            // - si aucune carte jouée -> pas d'attaque, donc pas de défense => tour change au début de EndTurn
            MatchPhaseType.StandBy => HasActiveCardOnBoard(match)
                ? new NextStep(new AttackPhase(), curPos)
                : new NextStep(new EndTurnPhase(), Swap(curPos)),
            // Attack:
            // - si défense possible => tour change au début de Defense
            // - sinon => tour change au début de EndTurn
            MatchPhaseType.Attack => match.HasPendingDefense
                ? new NextStep(new DefensePhase(), Swap(curPos))
                : new NextStep(new EndTurnPhase(), Swap(curPos)),

            // Defense:
            // tour NE change PAS ici (il a déjà changé en entrant en Defense)
            MatchPhaseType.Defense => new NextStep(new EndTurnPhase(), curPos),

            // EndTurn:
            // tour NE change PAS ici (il a déjà changé en entrant en EndTurn)
            MatchPhaseType.EndTurn => new NextStep(new StandByPhase(), curPos),

            _ => new NextStep(new StandByPhase(), curPos)
        };
    }
    private static bool HasActiveCardOnBoard(Match match)
    {
        Player currentPlayer = match.GetCurrentPlayer();

        return currentPlayer.Board
            .EnumerateSlots()
            .Select(kv => kv.Value)
            .Any(card => card.States.Value == CardStates.Active.Value);
    }
    private static int Swap(int pos) => pos == 1 ? 2 : 1;
}