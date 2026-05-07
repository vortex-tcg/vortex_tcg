using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Domain.Service;

public class ChampionDeathServiceTests
{
    [Fact]
    public void CheckChampionDeath_DoesNotAddEvent_WhenChampionAlive()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();
        Player player = match.Player1;
        player.Champion.Hp = new ChampionHp(1);

        match.PullEvents();
        new ChampionDeathService().CheckChampionDeath(match, player);

        IReadOnlyList<game.Domaine.Interface.IEvent> events = match.PullEvents();
        Assert.Empty(events);
    }

    [Fact]
    public void CheckChampionDeath_AddsMatchEndedEvent_WhenChampionDead()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();
        Player loser = match.Player1;
        loser.Champion.Hp = new ChampionHp(0);

        match.PullEvents();
        new ChampionDeathService().CheckChampionDeath(match, loser);

        IReadOnlyList<game.Domaine.Interface.IEvent> events = match.PullEvents();
        DomainEvent? endEvent = events.OfType<DomainEvent>()
            .FirstOrDefault(e => e.Name == MatchEvent.MATCH_ENDED);

        Assert.NotNull(endEvent);
    }

    [Fact]
    public void CheckChampionDeath_WinnerIsOpponent_WhenCurrentPlayerDies()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();
        Player loser = match.Player1;
        Player expectedWinner = match.Player2;
        loser.Champion.Hp = new ChampionHp(-5);

        match.PullEvents();
        new ChampionDeathService().CheckChampionDeath(match, loser);

        IReadOnlyList<game.Domaine.Interface.IEvent> events = match.PullEvents();
        DomainEvent endEvent = events.OfType<DomainEvent>()
            .First(e => e.Name == MatchEvent.MATCH_ENDED);
        MatchEndedData data = endEvent.GetData<MatchEndedData>();

        Assert.Equal((Guid)expectedWinner.UserId, data.WinnerUserId);
        Assert.Equal((Guid)loser.UserId, data.LoserUserId);
    }

    [Fact]
    public void CheckChampionDeath_Throws_WhenMatchIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ChampionDeathService().CheckChampionDeath(null!, MatchHelpers.MakePlayer()));
    }

    [Fact]
    public void CheckChampionDeath_Throws_WhenPlayerIsNull()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();
        Assert.Throws<ArgumentNullException>(() =>
            new ChampionDeathService().CheckChampionDeath(match, null!));
    }
}
