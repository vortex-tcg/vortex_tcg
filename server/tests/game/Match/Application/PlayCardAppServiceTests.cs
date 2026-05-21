using game.Application.Service;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Event.Action;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Moq;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Domain.Application;

[Collection("ApplicationTests")]
public class PlayCardAppServiceTests : IDisposable
{
    private readonly Mock<IClientProxy> _mockProxy;

    public PlayCardAppServiceTests()
    {
        _mockProxy = AppServiceHelpers.ConfigureCallManager();
    }

    public void Dispose() { }

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
    public async Task PlayCardAsync_PlacesCardOnBoard()
    {
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch();

        await PlayCardAppService.PlayCardAsync(match, p1Id, card.GameCardId.Value, 1);

        Assert.NotNull(match.Player1.Board.GetCardAtPosition(1));
    }

    [Fact]
    public async Task PlayCardAsync_SendsSignalR_WhenCardPlayed()
    {
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch();

        await PlayCardAppService.PlayCardAsync(match, p1Id, card.GameCardId.Value, 1);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task PlayCardAsync_Throws_WhenNotEnoughGold()
    {
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch(cardCost: 10, championGold: 3);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            PlayCardAppService.PlayCardAsync(match, p1Id, card.GameCardId.Value, 1));
    }

    [Fact]
    public async Task PlayCardAsync_SkipsNonCardPlayedEvents_InLoop()
    {
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch();
        match.AddEvent(new DomainEvent("OTHER_EVENT", new object()));

        await PlayCardAppService.PlayCardAsync(match, p1Id, card.GameCardId.Value, 1);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public void PlayCard_AlwaysProducesCardPlayedEvent_GuardIsNeverTriggered()
    {
        // `if (playedEvent == null) return;` in PlayCardAppService is dead code:
        // Match.PlayCard unconditionally adds a CARD_PLAYED event.
        (MatchAggregate match, UserId p1Id, GameCardDto card) = BuildStandByMatch();
        match.PullEvents(); // drain any pre-existing events

        match.PlayCard(p1Id, card.GameCardId.Value, 1);

        var events = match.PullEvents();
        Assert.Contains(events, ev => ev.Name == PlayCardEvent.CARD_PLAYED);
    }
}
