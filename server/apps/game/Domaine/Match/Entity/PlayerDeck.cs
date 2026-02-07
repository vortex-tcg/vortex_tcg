namespace game.Domaine.Match.Entity;



public sealed class PlayerDeck
{
    private readonly List<GameCardDto> _cards;

    public PlayerDeck(IEnumerable<GameCardDto> cards)
    {
        _cards = new List<GameCardDto>(cards);
    }

    public int Count => _cards.Count;

    public void Shuffle(Random rng)
    {
        for (int i = _cards.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
    }

    public GameCardDto? DrawOne()
    {
        if (_cards.Count == 0) return null;
        GameCardDto top = _cards[0];
        _cards.RemoveAt(0);
        return top;
    }
}

public sealed class Hand
{
    private readonly List<GameCardDto> _cards = new();
    public IReadOnlyList<GameCardDto> Cards => _cards;

    public void Add(GameCardDto card) => _cards.Add(card);
}

public sealed class Board { }
public sealed class Graveyard { }
