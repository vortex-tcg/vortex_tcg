using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;
using MatchAggregate = game.Domaine.Match.Agregate.Match;
using VCardState = game.Domaine.Match.ValueObject.CardState;

namespace game.Tests.Domain.Service;

public class PlayCardServiceTests
{
    private static (MatchAggregate match, UserId p1Id, GameCardDto card) BuildStandByMatch(
        int cardCost = 2,
        int championGold = 5)
    {
        UserId p1Id = new UserId(Guid.NewGuid());
        GameCardDto card = MatchHelpers.MakeCard(1, cost: cardCost);
        GameChampionDto champion = MatchHelpers.MakeChampion(gold: championGold);

        Player p1 = MatchHelpers.MakePlayer(userId: p1Id, champion: champion);
        p1.Hand.Add(card);

        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>(p1: p1);
        return (match, p1Id, card);
    }

    [Fact]
    public void PlayCard_PlacesCardOnBoard()
    {
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch(cardCost: 2, championGold: 5);

        PlayCardService.PlayCard(match, p1Id, card.GameCardId.Value, boardPosition: 1);

        Assert.NotNull(match.Player1.Board.GetCardAtPosition(1));
        Assert.Equal(0, match.Player1.Hand.Count);
    }

    [Fact]
    public void PlayCard_DeductsGoldFromChampion()
    {
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch(cardCost: 3, championGold: 5);

        PlayCardService.PlayCard(match, p1Id, card.GameCardId.Value, boardPosition: 1);

        Assert.Equal(2, match.Player1.Champion.Gold.Value);
    }

    [Fact]
    public void PlayCard_SetsCardStateToSleeping()
    {
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch();

        PlayCardService.PlayCard(match, p1Id, card.GameCardId.Value, boardPosition: 1);

        GameCardDto? placed = match.Player1.Board.GetCardAtPosition(1);
        Assert.Equal(CardStates.Sleeping.Value, placed!.States.Value);
    }

    [Fact]
    public void PlayCard_Throws_WhenNotStandByPhase()
    {
        UserId p1Id = new UserId(Guid.NewGuid());
        GameCardDto card = MatchHelpers.MakeCard(1, cost: 1);
        Player p1 = MatchHelpers.MakePlayer(userId: p1Id, champion: MatchHelpers.MakeChampion(gold: 5));
        p1.Hand.Add(card);

        MatchAggregate match = MatchHelpers.MakeMatchInPhase<AttackPhase>(p1: p1);

        Assert.Throws<InvalidOperationException>(() =>
            PlayCardService.PlayCard(match, p1Id, card.GameCardId.Value, boardPosition: 1));
    }

    [Fact]
    public void PlayCard_Throws_WhenNotCurrentPlayer()
    {
        (MatchAggregate match, _, GameCardDto card) = BuildStandByMatch();
        UserId wrongUser = new UserId(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() =>
            PlayCardService.PlayCard(match, wrongUser, card.GameCardId.Value, boardPosition: 1));
    }

    [Fact]
    public void PlayCard_Throws_WhenNotEnoughGold()
    {
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch(cardCost: 10, championGold: 3);

        Assert.Throws<InvalidOperationException>(() =>
            PlayCardService.PlayCard(match, p1Id, card.GameCardId.Value, boardPosition: 1));
    }

    [Fact]
    public void PlayCard_Throws_WhenSlotOccupied()
    {
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch();
        match.Player1.Board.Place(1, MatchHelpers.MakeCard(99));

        Assert.Throws<InvalidOperationException>(() =>
            PlayCardService.PlayCard(match, p1Id, card.GameCardId.Value, boardPosition: 1));
    }

    [Fact]
    public void PlayCard_Throws_WhenCardNotInHand()
    {
        (MatchAggregate match, UserId p1Id, _) = BuildStandByMatch();

        Assert.Throws<InvalidOperationException>(() =>
            PlayCardService.PlayCard(match, p1Id, gameCardId: 999, boardPosition: 1));
    }

    [Fact]
    public void PlayCard_ReturnsCorrectDto()
    {
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch(cardCost: 2, championGold: 5);

        PlayCardData data = PlayCardService.PlayCard(match, p1Id, card.GameCardId.Value, boardPosition: 1);

        Assert.Equal(3, data.PlayerGold);
        Assert.Equal(1, data.BoardPosition);
        Assert.Equal(card.GameCardId.Value, data.GameCardId);
    }
}
