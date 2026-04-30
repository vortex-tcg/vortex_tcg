namespace game.Domaine.Match.Service;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;



public static class EndTurnService
{
    public static void Apply(Match match)
    {
        Player currentPlayer = match.GetCurrentPlayer();
        Player opponentPlayer = match.GetOpponentPlayer();

        ResetBoard(currentPlayer);
        ResetBoard(opponentPlayer);
    }

    private static void ResetBoard(Player player)
    {
        foreach (KeyValuePair<int, GameCardDto> entry in player.Board.EnumerateSlots())
        {
            GameCardDto card = entry.Value;

            if (ShouldBeReactivated(card))
            {
                card.States = CardStates.Active;
            }
        }
    }

    private static bool ShouldBeReactivated(GameCardDto card)
    {
        return card.States.Value != CardStates.Active.Value;
    }
}