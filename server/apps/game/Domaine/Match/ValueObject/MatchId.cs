namespace game.Domaine.Match.ValueObject;

public class MatchId
{
    public Guid Value { get; }

    public MatchId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MatchId can't be empty", nameof(value));

        Value = value;
    }

    public MatchId()
    {
        Value = Guid.NewGuid();
    }
    public static implicit operator Guid(MatchId id) => id.Value;
    public static explicit operator UserId(MatchId value) => new(value);

    public override string ToString() => Value.ToString();
}