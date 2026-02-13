namespace game.Domaine.Match.Entity;

using ValueObject;


public sealed class Match
{
    public MatchId MatchId { get; } = new MatchId();

    public Player Player1 { get; }
    public Player Player2 { get; }

    public bool IsFinished { get; private set; }

    public Match(Player p1, Player p2)
    {
        Player1 = p1;
        Player2 = p2;
    }

    public bool HasUser(UserId userId)
        => Player1.UserId.Equals(userId) || Player2.UserId.Equals(userId);

    public void Start()
    {
        // TODO: état de tour, mana, etc.
        IsFinished = false;
    }

    public void Finish() => IsFinished = true;
}
