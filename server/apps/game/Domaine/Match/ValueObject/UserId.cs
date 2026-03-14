namespace game.Domaine.Match.ValueObject;

public readonly record struct UserId
{
    public Guid Value { get; }

    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId can't be empty", nameof(value));

        Value = value;
    }

    public static implicit operator Guid(UserId id) => id.Value;
    public static explicit operator UserId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}