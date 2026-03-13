using System.Collections.Generic;
using game.Domaine.Match.Entity;

namespace game.Domaine.Match.Service;

public static class DrawCardsService
{
    public static List<GameCardDto> DrawCards(Player player, int count)
    {
        List<GameCardDto> drawn = new List<GameCardDto>(count);

        int i = 0;
        while (i < count)
        {
            GameCardDto? card = player.Deck.DrawOne();
            if (card == null)
            {
                break;
            }

            player.Hand.Add(card);
            drawn.Add(card);
            i++;
        }

        return drawn;
    }
}