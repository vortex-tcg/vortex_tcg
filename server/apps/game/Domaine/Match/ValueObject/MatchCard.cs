using game.Domaine.Match.Entity;

namespace game.Domaine.Match.ValueObject;

public readonly record struct CardId
{
    public Guid Value { get; }

    public CardId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CardId can't be empty.", nameof(value));

        Value = value;
    }

    public static implicit operator Guid(CardId id) => id.Value;
    public static explicit operator CardId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}

public readonly record struct GameCardId
{
    public int Value { get; }

    public GameCardId(int value)
    {
        if (value <= 0)
            throw new ArgumentException("GameCardId must be > 0.", nameof(value));

        Value = value;
    }

    public static implicit operator int(GameCardId id) => id.Value;
    public static explicit operator GameCardId(int value) => new(value);

    public override string ToString() => Value.ToString();
}

public readonly record struct CardName
{
    public string Value { get; }

    public CardName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CardName can't be empty.", nameof(value));

        Value = value.Trim();
    }

    public static implicit operator string(CardName x) => x.Value;
    public static explicit operator CardName(string value) => new(value);

    public override string ToString() => Value;
}

public readonly record struct CardHpValue
{
    public int Value { get; }

    public CardHpValue(int value)
    {
        Value = value;
    }

    public static implicit operator int(CardHpValue x) => x.Value;
    public static explicit operator CardHpValue(int value) => new(value);

    public override string ToString() => Value.ToString();
}
public readonly record struct CardAttackValue
{
    public int Value { get; }

    public CardAttackValue(int value)
    {
        if (value < 0)
            throw new ArgumentException("Attack can't be negative.", nameof(value));

        Value = value;
    }

    public static implicit operator int(CardAttackValue x) => x.Value;
    public static explicit operator CardAttackValue(int value) => new(value);

    public override string ToString() => Value.ToString();
}

public readonly record struct CardCostValue
{
    public int Value { get; }

    public CardCostValue(int value)
    {
        if (value < 0)
            throw new ArgumentException("Cost can't be negative.", nameof(value));

        Value = value;
    }

    public static implicit operator int(CardCostValue x) => x.Value;
    public static explicit operator CardCostValue(int value) => new(value);

    public override string ToString() => Value.ToString();
}

public readonly record struct CardDescription
{
    public string Value { get; }

    public CardDescription(string value)
    {
        Value = value?.Trim() ?? string.Empty;
    }

    public static implicit operator string(CardDescription x) => x.Value;
    public static explicit operator CardDescription(string value) => new(value);

    public override string ToString() => Value;
}

public readonly record struct CardImageUrl
{
    public string Value { get; }

    public CardImageUrl(string value)
    {
        Value = value?.Trim() ?? string.Empty;
    }

    public static implicit operator string(CardImageUrl x) => x.Value;
    public static explicit operator CardImageUrl(string value) => new(value);

    public override string ToString() => Value;
}


public readonly record struct CardClasses
{
    public IReadOnlyList<string> Value { get; }

    public CardClasses(IReadOnlyList<string> value)
    {
        Value = value ?? Array.Empty<string>();
    }

    public override string ToString() => $"[{string.Join(",", Value)}]";
}

public enum CardState
{
    Sleeping = 0,
    Active = 1,
    Attacking = 2,
    Defending = 3
}
public readonly struct CardStates
{
    public CardState Value { get; }

    public CardStates(CardState value)
    {
        Value = value;
    }
    public CardStates(int raw)
    {
        Value = (CardState)raw;
    }
    public static CardStates Sleeping => new(CardState.Sleeping);
    public static CardStates Active => new(CardState.Active);
    public static CardStates Attacking => new(CardState.Attacking);
    public static CardStates Defending => new(CardState.Defending);

    public override string ToString() => Value.ToString();
}
