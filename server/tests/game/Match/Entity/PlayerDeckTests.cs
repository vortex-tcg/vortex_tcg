using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Domain.Entity;

public class PlayerDeckTests
{
    [Fact]
    public void DrawOne_ReturnsTopCard()
    {
        GameCardDto card1 = MatchHelpers.MakeCard(1);
        GameCardDto card2 = MatchHelpers.MakeCard(2);
        PlayerDeck deck = new PlayerDeck([card1, card2]);

        GameCardDto? drawn = deck.DrawOne();

        Assert.NotNull(drawn);
        Assert.Equal(1, drawn!.GameCardId.Value);
        Assert.Equal(1, deck.Count);
    }

    [Fact]
    public void DrawOne_ReturnsNullWhenEmpty()
    {
        PlayerDeck deck = new PlayerDeck([]);

        GameCardDto? drawn = deck.DrawOne();

        Assert.Null(drawn);
    }

    [Fact]
    public void Shuffle_ChangesOrder()
    {
        List<GameCardDto> cards = Enumerable.Range(1, 10).Select(i => MatchHelpers.MakeCard(i)).ToList();
        PlayerDeck deck = new PlayerDeck(cards);

        deck.Shuffle(new Random(42));

        List<GameCardDto> drawn = new List<GameCardDto>();
        for (int i = 0; i < 10; i++)
        {
            drawn.Add(deck.DrawOne()!);
        }

        bool sameOrder = drawn.Select(c => c.GameCardId.Value).SequenceEqual(Enumerable.Range(1, 10));
        Assert.False(sameOrder);
    }

    [Fact]
    public void Shuffle_PreservesCount()
    {
        List<GameCardDto> cards = Enumerable.Range(1, 5).Select(i => MatchHelpers.MakeCard(i)).ToList();
        PlayerDeck deck = new PlayerDeck(cards);

        deck.Shuffle(new Random(1));

        Assert.Equal(5, deck.Count);
    }
}

public class BoardTests
{
    [Fact]
    public void Place_AndGetCardAtPosition()
    {
        Board board = new Board();
        GameCardDto card = MatchHelpers.MakeCard(1);
        board.Place(1, card);

        GameCardDto? retrieved = board.GetCardAtPosition(1);
        Assert.NotNull(retrieved);
        Assert.Equal(1, retrieved!.GameCardId.Value);
    }

    [Fact]
    public void IsSlotFree_ReturnsTrueWhenEmpty()
    {
        Board board = new Board();

        Assert.True(board.IsSlotFree(1));
    }

    [Fact]
    public void IsSlotFree_ReturnsFalseAfterPlace()
    {
        Board board = new Board();
        board.Place(1, MatchHelpers.MakeCard(1));

        Assert.False(board.IsSlotFree(1));
    }

    [Fact]
    public void RemoveCardAtPosition_RemovesCard()
    {
        Board board = new Board();
        board.Place(1, MatchHelpers.MakeCard(1));
        board.RemoveCardAtPosition(1);

        Assert.Null(board.GetCardAtPosition(1));
        Assert.Equal(0, board.Count);
    }

    [Fact]
    public void TakeCardAtPosition_RemovesAndReturnsCard()
    {
        Board board = new Board();
        GameCardDto card = MatchHelpers.MakeCard(1);
        board.Place(1, card);

        GameCardDto? taken = board.TakeCardAtPosition(1);

        Assert.NotNull(taken);
        Assert.Equal(1, taken!.GameCardId.Value);
        Assert.Equal(0, board.Count);
    }

    [Fact]
    public void TakeCardAtPosition_ReturnsNullWhenSlotEmpty()
    {
        Board board = new Board();

        GameCardDto? taken = board.TakeCardAtPosition(99);

        Assert.Null(taken);
    }

    [Fact]
    public void EnumerateSlots_ReturnsAllPlacedCards()
    {
        Board board = new Board();
        board.Place(1, MatchHelpers.MakeCard(1));
        board.Place(2, MatchHelpers.MakeCard(2));

        List<KeyValuePair<int, GameCardDto>> slots = board.EnumerateSlots().ToList();

        Assert.Equal(2, slots.Count);
    }
}

public class HandTests
{
    [Fact]
    public void Add_IncreasesCount()
    {
        Hand hand = new Hand();
        hand.Add(MatchHelpers.MakeCard(1));

        Assert.Equal(1, hand.Count);
    }

    [Fact]
    public void Remove_RemovesCard()
    {
        Hand hand = new Hand();
        GameCardDto card = MatchHelpers.MakeCard(1);
        hand.Add(card);

        bool removed = hand.Remove(card);

        Assert.True(removed);
        Assert.Equal(0, hand.Count);
    }

    [Fact]
    public void Remove_ReturnsFalseWhenCardNotInHand()
    {
        Hand hand = new Hand();
        GameCardDto card = MatchHelpers.MakeCard(1);

        bool removed = hand.Remove(card);

        Assert.False(removed);
    }

    [Fact]
    public void Clear_EmptiesHand()
    {
        Hand hand = new Hand();
        hand.Add(MatchHelpers.MakeCard(1));
        hand.Add(MatchHelpers.MakeCard(2));
        hand.Clear();

        Assert.Equal(0, hand.Count);
    }
}

public class GraveyardTests
{
    [Fact]
    public void Add_IncreasesCount()
    {
        Graveyard yard = new Graveyard();
        yard.Add(MatchHelpers.MakeCard(1));

        Assert.Equal(1, yard.Count);
    }

    [Fact]
    public void GetCards_ReturnsAllAddedCards()
    {
        Graveyard yard = new Graveyard();
        yard.Add(MatchHelpers.MakeCard(1));
        yard.Add(MatchHelpers.MakeCard(2));

        IReadOnlyList<GameCardDto> cards = yard.GetCards();

        Assert.Equal(2, cards.Count);
    }
}

public class BoardExtendedTests
{
    [Fact]
    public void HasCardAtPosition_ReturnsTrueWhenOccupied()
    {
        Board board = new Board();
        board.Place(1, MatchHelpers.MakeCard(1));

        Assert.True(board.HasCardAtPosition(1));
    }

    [Fact]
    public void HasCardAtPosition_ReturnsFalseWhenEmpty()
    {
        Board board = new Board();

        Assert.False(board.HasCardAtPosition(1));
    }

    [Fact]
    public void TryGet_ReturnsTrueAndCard_WhenSlotOccupied()
    {
        Board board = new Board();
        GameCardDto card = MatchHelpers.MakeCard(1);
        board.Place(1, card);

        bool result = board.TryGet(1, out GameCardDto? retrieved);

        Assert.True(result);
        Assert.Equal(1, retrieved!.GameCardId.Value);
    }

    [Fact]
    public void TryGet_ReturnsFalse_WhenSlotEmpty()
    {
        Board board = new Board();

        bool result = board.TryGet(99, out GameCardDto? retrieved);

        Assert.False(result);
        Assert.Null(retrieved);
    }
}

public class StandByPhaseTests
{
    [Fact]
    public void OnStartPhase_WithDeckCard_DrawsCardToHand()
    {
        GameCardDto card = MatchHelpers.MakeCard(1);
        Player p1 = MatchHelpers.MakePlayer(deck: new[] { card });
        MatchAggregate match = MatchHelpers.MakeMatch(p1: p1);
        StandByPhase phase = new StandByPhase();

        phase.OnStartPhase(match, CancellationToken.None);

        Assert.Equal(1, match.Player1.Hand.Count);
    }

    [Fact]
    public void OnStartPhase_WithEmptyDeck_HandRemainsEmpty()
    {
        Player p1 = MatchHelpers.MakePlayer(deck: Array.Empty<GameCardDto>());
        MatchAggregate match = MatchHelpers.MakeMatch(p1: p1);
        StandByPhase phase = new StandByPhase();

        phase.OnStartPhase(match, CancellationToken.None);

        Assert.Equal(0, match.Player1.Hand.Count);
    }
}

public class DefensePhaseTests
{
    [Fact]
    public void Type_IsDefense()
    {
        DefensePhase phase = new DefensePhase();

        Assert.Equal(MatchPhaseType.Defense, phase.Type);
    }

    [Fact]
    public void OnStartPhase_DoesNotThrow()
    {
        DefensePhase phase = new DefensePhase();
        MatchAggregate match = MatchHelpers.MakeMatch();

        Exception? ex = Record.Exception(() => phase.OnStartPhase(match, CancellationToken.None));

        Assert.Null(ex);
    }

    [Fact]
    public void OnEndPhase_DoesNotThrow()
    {
        DefensePhase phase = new DefensePhase();
        MatchAggregate match = MatchHelpers.MakeMatch();

        Exception? ex = Record.Exception(() => phase.OnEndPhase(match, CancellationToken.None));

        Assert.Null(ex);
    }
}
