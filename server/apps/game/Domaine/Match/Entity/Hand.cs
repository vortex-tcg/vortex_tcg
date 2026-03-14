namespace game.Domaine.Match.Entity;

public sealed class Hand
{
    private readonly List<GameCardDto> _cards = new();

    public IReadOnlyList<GameCardDto> Cards => _cards;
    public int Count => _cards.Count;

    public void Add(GameCardDto card) => _cards.Add(card);

    public bool Remove(GameCardDto card) => _cards.Remove(card);

    public void Clear() => _cards.Clear();
}
