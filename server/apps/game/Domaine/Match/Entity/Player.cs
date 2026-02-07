using VortexTCG.Game.Object;

namespace game.Domaine.Match.Entity;

using game.Domaine.Match.ValueObject;


public sealed class Player
{
    public UserId UserId { get; }
    public DeckId DeckId { get; }

    public PlayerDeck Deck { get; }
    public Hand Hand { get; }
    public Board Board { get; }
    public Graveyard Graveyard { get; }

    public GameChampionDto Champion { get; private set; }

    public Player(UserId userId, DeckId deckId, PlayerDeck deck, GameChampionDto champion)
    {
        UserId = userId;
        DeckId = deckId;
        Deck = deck;
        Hand = new Hand();
        Board = new Board();
        Graveyard = new Graveyard();
        Champion = champion;
    }
}
