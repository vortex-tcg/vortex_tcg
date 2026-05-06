using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Tests.Helpers;

namespace game.Tests.Domain.Service;

public class DrawCardsServiceTests
{
    [Fact]
    public void DrawCards_DrawsRequestedCount()
    {
        List<GameCardDto> deckCards = Enumerable.Range(1, 10).Select(i => MatchHelpers.MakeCard(i)).ToList();
        Player player = MatchHelpers.MakePlayer(deck: deckCards);

        List<GameCardDto> drawn = DrawCardsService.DrawCards(player, 3);

        Assert.Equal(3, drawn.Count);
        Assert.Equal(3, player.Hand.Count);
        Assert.Equal(7, player.Deck.Count);
    }

    [Fact]
    public void DrawCards_DrawsAllCardsWhenDeckHasLess()
    {
        List<GameCardDto> deckCards = [MatchHelpers.MakeCard(1), MatchHelpers.MakeCard(2)];
        Player player = MatchHelpers.MakePlayer(deck: deckCards);

        List<GameCardDto> drawn = DrawCardsService.DrawCards(player, 5);

        Assert.Equal(2, drawn.Count);
        Assert.Equal(2, player.Hand.Count);
        Assert.Equal(0, player.Deck.Count);
    }

    [Fact]
    public void DrawCards_ReturnsEmpty_WhenDeckIsEmpty()
    {
        Player player = MatchHelpers.MakePlayer(deck: []);

        List<GameCardDto> drawn = DrawCardsService.DrawCards(player, 3);

        Assert.Empty(drawn);
        Assert.Equal(0, player.Hand.Count);
    }

    [Fact]
    public void DrawCards_AddsCardsToHand()
    {
        List<GameCardDto> deckCards = Enumerable.Range(1, 5).Select(i => MatchHelpers.MakeCard(i)).ToList();
        Player player = MatchHelpers.MakePlayer(deck: deckCards);

        DrawCardsService.DrawCards(player, 2);

        Assert.Equal(2, player.Hand.Count);
        Assert.Equal(1, player.Hand.Cards[0].GameCardId.Value);
        Assert.Equal(2, player.Hand.Cards[1].GameCardId.Value);
    }

    [Fact]
    public void DrawCards_WithZeroCount_DrawsNothing()
    {
        List<GameCardDto> deckCards = Enumerable.Range(1, 5).Select(i => MatchHelpers.MakeCard(i)).ToList();
        Player player = MatchHelpers.MakePlayer(deck: deckCards);

        List<GameCardDto> drawn = DrawCardsService.DrawCards(player, 0);

        Assert.Empty(drawn);
        Assert.Equal(0, player.Hand.Count);
        Assert.Equal(5, player.Deck.Count);
    }
}
