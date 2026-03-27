namespace game.Domaine.Match.ValueObject;

public readonly record struct DeckId
{
    public Guid Value { get; }

    public DeckId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DeckId can't be empty.", nameof(value));

        Value = value;
    }

    public static implicit operator Guid(DeckId id) => id.Value;
    public static explicit operator DeckId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}