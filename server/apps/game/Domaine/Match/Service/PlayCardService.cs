using game.Domaine.Match.DTO;

namespace game.Domaine.Match.Service;
using System;
using System.Threading;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;


public static class PlayCardService
{
    public static PlayCardData PlayCard(
        Agregate.Match match,
        UserId userId,
        int gameCardId,
        int boardPosition,
        CancellationToken ct = default)
    { 
        if (match.CurrentPhase.Type != MatchPhaseType.StandBy)
            throw new InvalidOperationException("PlayCard allowed only in StandBy phase.");
        var player = match.GetCurrentPlayer();
        if (!player.UserId.Equals(userId))
            throw new InvalidOperationException("Not your turn.");

        var opponent = match.GetOpponentPlayer();
        GameCardDto card = FindCardInHand(player, gameCardId);
        int cost = card.Cost.Value;
        int gold = player.Champion.Gold.Value;
        if (gold < cost)
            throw new InvalidOperationException("Not enough gold.");
        if (!player.Board.IsSlotFree(boardPosition))
            throw new InvalidOperationException("Board position is not free.");
        bool removed = player.Hand.Remove(card);
        if (!removed)
            throw new InvalidOperationException("Card not in hand."); 

        player.Champion.Gold = new game.Domaine.Match.ValueObject.ChampionGold(gold - cost);
        card.States = CardStates.Sleeping; 
        player.Board.Place(boardPosition, card);
        return new PlayCardData(
            match.MatchId.Value,
            (Guid)player.UserId,
            (Guid)opponent.UserId,
            player.Champion.Gold.Value,
            opponent.Champion.Gold.Value,
            boardPosition,
            card.GameCardId.Value,
            card
        );
    }

    private static GameCardDto FindCardInHand(Entity.Player player, int gameCardId)
    {
        int i = 0;
        while (i < player.Hand.Cards.Count)
        {
            GameCardDto c = player.Hand.Cards[i];
            if (c.GameCardId.Value == gameCardId)
                return c;
            i++;
        }
        throw new InvalidOperationException("Card not found in hand.");
    }
}
