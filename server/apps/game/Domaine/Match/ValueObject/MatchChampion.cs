namespace game.Domaine.Match.ValueObject;

public readonly record struct ChampionId
{
    public Guid Value { get; }
    public ChampionId(Guid value)
    {
        if (value == Guid.Empty) throw new ArgumentException("ChampionId can't be empty.", nameof(value));
        Value = value;
    }
    public static implicit operator Guid(ChampionId id) => id.Value;
    public static explicit operator ChampionId(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

public readonly record struct ChampionBaseHp
{
    public int Value { get; }
    public ChampionBaseHp(int value)
    {
        if (value <= 0) throw new ArgumentException("ChampionBaseHp must be > 0.", nameof(value));
        Value = value;
    }
    public static implicit operator int(ChampionBaseHp x) => x.Value;
    public static explicit operator ChampionBaseHp(int value) => new(value);
}

public readonly record struct ChampionHp
{
    public int Value { get; }
    public ChampionHp(int value)
    {
        if (value < 0) throw new ArgumentException("ChampionHp can't be negative.", nameof(value));
        Value = value;
    }
    public static implicit operator int(ChampionHp x) => x.Value;
    public static explicit operator ChampionHp(int value) => new(value);
}

public readonly record struct ChampionBaseGold
{
    public int Value { get; }
    public ChampionBaseGold(int value)
    {
        if (value < 0) throw new ArgumentException("ChampionBaseGold can't be negative.", nameof(value));
        Value = value;
    }
    public static implicit operator int(ChampionBaseGold x) => x.Value;
    public static explicit operator ChampionBaseGold(int value) => new(value);
}

public readonly record struct ChampionGold
{
    public int Value { get; }
    public ChampionGold(int value)
    {
        if (value < 0) throw new ArgumentException("ChampionGold can't be negative.", nameof(value));
        Value = value;
    }
    public static implicit operator int(ChampionGold x) => x.Value;
    public static explicit operator ChampionGold(int value) => new(value);
}

public readonly record struct ChampionSecondaryCurrency
{
    public int Value { get; }
    public ChampionSecondaryCurrency(int value)
    {
        if (value < 0) throw new ArgumentException("SecondaryCurrency can't be negative.", nameof(value));
        Value = value;
    }
    public static implicit operator int(ChampionSecondaryCurrency x) => x.Value;
    public static explicit operator ChampionSecondaryCurrency(int value) => new(value);
}

public readonly record struct ChampionSecondaryCurrencyName
{
    public string Value { get; }
    public ChampionSecondaryCurrencyName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SecondaryCurrencyName can't be empty.", nameof(value));
        Value = value.Trim();
    }
    public static implicit operator string(ChampionSecondaryCurrencyName x) => x.Value;
    public static explicit operator ChampionSecondaryCurrencyName(string value) => new(value);
}

public readonly record struct ChampionFatigueCounter
{
    public int Value { get; }
    public ChampionFatigueCounter(int value)
    {
        if (value < 0) throw new ArgumentException("FatigueCounter can't be negative.", nameof(value));
        Value = value;
    }
    public static implicit operator int(ChampionFatigueCounter x) => x.Value;
    public static explicit operator ChampionFatigueCounter(int value) => new(value);
}
