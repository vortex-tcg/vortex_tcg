using game.Domaine.Match.ValueObject;

namespace game.Domaine.Match.Service;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;


public static class EndTurnService
{
    public static void Apply(Match match)
    {
        Player current = match.GetCurrentPlayer();
        Player opponent = match.GetOpponentPlayer();

        foreach (var kv in current.Board.EnumerateSlots())
        {
            GameCardDto card = kv.Value;
            if (card.States.Value == CardStates.Sleeping.Value)
                card.States = CardStates.Active;
        }
        foreach (var kv in opponent.Board.EnumerateSlots())
        {
            GameCardDto card = kv.Value;
            if (card.States.Value == CardStates.Attacking.Value)
                card.States = CardStates.Active;
        }
        foreach (var kv in current.Board.EnumerateSlots())
        {
            GameCardDto card = kv.Value;
            if (card.States.Value == CardStates.Defending.Value)
                card.States = CardStates.Active;
        }
    }
}
