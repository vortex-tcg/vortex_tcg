using game.Domaine.Match.Agregate;
using game.Domaine.Match.Service;
using game.Tests.Helpers;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Domain.Service;

public class InitMatchServiceTests
{
    private static MatchAggregate BuildMatchWithDecks(int p1DeckSize = 10, int p2DeckSize = 10)
    {
        List<game.Domaine.Match.Entity.GameCardDto> p1Deck =
            Enumerable.Range(1, p1DeckSize).Select(i => MatchHelpers.MakeCard(i)).ToList();
        List<game.Domaine.Match.Entity.GameCardDto> p2Deck =
            Enumerable.Range(p1DeckSize + 1, p2DeckSize).Select(i => MatchHelpers.MakeCard(i)).ToList();

        game.Domaine.Match.Entity.Player p1 = MatchHelpers.MakePlayer(deck: p1Deck);
        game.Domaine.Match.Entity.Player p2 = MatchHelpers.MakePlayer(deck: p2Deck);

        return MatchHelpers.MakeMatch(p1: p1, p2: p2);
    }

    [Fact]
    public void Init_DrawsSixCardsForPlayer1()
    {
        MatchAggregate match = BuildMatchWithDecks();

        InitMatchService.Init(match);

        Assert.Equal(6, match.Player1.Hand.Count);
    }

    [Fact]
    public void Init_DrawsFiveCardsForPlayer2()
    {
        MatchAggregate match = BuildMatchWithDecks();

        InitMatchService.Init(match);

        Assert.Equal(5, match.Player2.Hand.Count);
    }

    [Fact]
    public void Init_SetsChampionGoldTo100ForBothPlayers()
    {
        MatchAggregate match = BuildMatchWithDecks();

        InitMatchService.Init(match);

        Assert.Equal(100, match.Player1.Champion.Gold.Value);
        Assert.Equal(100, match.Player2.Champion.Gold.Value);
    }

    [Fact]
    public void Init_ReturnsCorrectPlayerUserIds()
    {
        MatchAggregate match = BuildMatchWithDecks();

        MatchInitData data = InitMatchService.Init(match);

        Assert.Equal((Guid)match.Player1.UserId, data.Player1UserId);
        Assert.Equal((Guid)match.Player2.UserId, data.Player2UserId);
    }

    [Fact]
    public void Init_ReturnsCorrectPositions()
    {
        MatchAggregate match = BuildMatchWithDecks();

        MatchInitData data = InitMatchService.Init(match);

        Assert.Equal(1, data.Player1Position);
        Assert.Equal(2, data.Player2Position);
    }

    [Fact]
    public void Init_ReturnsCorrectMatchId()
    {
        MatchAggregate match = BuildMatchWithDecks();

        MatchInitData data = InitMatchService.Init(match);

        Assert.Equal(match.MatchId.Value, data.MatchId);
    }

    [Fact]
    public void Init_DrawsAtMostAvailableCards_WhenDeckIsTooSmall()
    {
        MatchAggregate match = BuildMatchWithDecks(p1DeckSize: 3, p2DeckSize: 3);

        InitMatchService.Init(match);

        Assert.Equal(3, match.Player1.Hand.Count);
        Assert.Equal(3, match.Player2.Hand.Count);
    }

    [Fact]
    public void Init_ReturnsDeckDataInMatchInitData()
    {
        MatchAggregate match = BuildMatchWithDecks();

        MatchInitData data = InitMatchService.Init(match);

        Assert.Equal(6, data.Player1DrawnCards.Count);
        Assert.Equal(5, data.Player2DrawnCards.Count);
    }

    [Fact]
    public void Init_ReturnsChampionDataForBothPlayers()
    {
        MatchAggregate match = BuildMatchWithDecks();

        MatchInitData data = InitMatchService.Init(match);

        Assert.NotNull(data.Player1Champion);
        Assert.NotNull(data.Player2Champion);
        Assert.Equal(match.Player1.Champion.Hp.Value, data.Player1Champion.Hp.Value);
        Assert.Equal(match.Player2.Champion.Hp.Value, data.Player2Champion.Hp.Value);
    }
}
