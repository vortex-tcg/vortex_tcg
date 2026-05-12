using game.Domaine.Match.ValueObject;
using Xunit;

namespace game.Tests.Domain.ValueObject;

public class ChampionIdTests
{
    [Fact]
    public void Constructor_ThrowsOnEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => new ChampionId(Guid.Empty));
    }

    [Fact]
    public void ExplicitCastFromGuid_CreatesChampionId()
    {
        Guid g = Guid.NewGuid();
        ChampionId id = (ChampionId)g;
        Assert.Equal(g, (Guid)id);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        Guid g = Guid.NewGuid();
        ChampionId id = new ChampionId(g);
        Assert.Equal(g.ToString(), id.ToString());
    }
}

public class ChampionBaseHpTests
{
    [Fact]
    public void Constructor_ThrowsOnZero()
    {
        Assert.Throws<ArgumentException>(() => new ChampionBaseHp(0));
    }

    [Fact]
    public void Constructor_ThrowsOnNegative()
    {
        Assert.Throws<ArgumentException>(() => new ChampionBaseHp(-1));
    }

    [Fact]
    public void ImplicitCastToInt_ReturnsValue()
    {
        ChampionBaseHp hp = new ChampionBaseHp(30);
        int cast = hp;
        Assert.Equal(30, cast);
    }

    [Fact]
    public void ExplicitCastFromInt_CreatesChampionBaseHp()
    {
        ChampionBaseHp hp = (ChampionBaseHp)30;
        Assert.Equal(30, hp.Value);
    }
}

public class ChampionHpTests
{
    [Fact]
    public void ImplicitCastToInt_ReturnsValue()
    {
        ChampionHp hp = new ChampionHp(25);
        int cast = hp;
        Assert.Equal(25, cast);
    }

    [Fact]
    public void ExplicitCastFromInt_CreatesChampionHp()
    {
        ChampionHp hp = (ChampionHp)25;
        Assert.Equal(25, hp.Value);
    }
}

public class ChampionBaseGoldTests
{
    [Fact]
    public void Constructor_ThrowsOnNegative()
    {
        Assert.Throws<ArgumentException>(() => new ChampionBaseGold(-1));
    }

    [Fact]
    public void ImplicitCastToInt_ReturnsValue()
    {
        ChampionBaseGold gold = new ChampionBaseGold(5);
        int cast = gold;
        Assert.Equal(5, cast);
    }

    [Fact]
    public void ExplicitCastFromInt_CreatesChampionBaseGold()
    {
        ChampionBaseGold gold = (ChampionBaseGold)5;
        Assert.Equal(5, gold.Value);
    }
}

public class ChampionGoldTests
{
    [Fact]
    public void Constructor_ThrowsOnNegative()
    {
        Assert.Throws<ArgumentException>(() => new ChampionGold(-1));
    }

    [Fact]
    public void ImplicitCastToInt_ReturnsValue()
    {
        ChampionGold gold = new ChampionGold(3);
        int cast = gold;
        Assert.Equal(3, cast);
    }

    [Fact]
    public void ExplicitCastFromInt_CreatesChampionGold()
    {
        ChampionGold gold = (ChampionGold)3;
        Assert.Equal(3, gold.Value);
    }
}

public class ChampionSecondaryCurrencyTests
{
    [Fact]
    public void Constructor_ThrowsOnNegative()
    {
        Assert.Throws<ArgumentException>(() => new ChampionSecondaryCurrency(-1));
    }

    [Fact]
    public void ImplicitCastToInt_ReturnsValue()
    {
        ChampionSecondaryCurrency sc = new ChampionSecondaryCurrency(2);
        int cast = sc;
        Assert.Equal(2, cast);
    }

    [Fact]
    public void ExplicitCastFromInt_CreatesChampionSecondaryCurrency()
    {
        ChampionSecondaryCurrency sc = (ChampionSecondaryCurrency)2;
        Assert.Equal(2, sc.Value);
    }
}

public class ChampionSecondaryCurrencyNameTests
{
    [Fact]
    public void Constructor_TrimsValue()
    {
        ChampionSecondaryCurrencyName name = new ChampionSecondaryCurrencyName("  Mana  ");
        Assert.Equal("Mana", name.Value);
    }

    [Fact]
    public void ImplicitCastToString_ReturnsValue()
    {
        ChampionSecondaryCurrencyName name = new ChampionSecondaryCurrencyName("Energy");
        string cast = name;
        Assert.Equal("Energy", cast);
    }

    [Fact]
    public void ExplicitCastFromString_CreatesName()
    {
        ChampionSecondaryCurrencyName name = (ChampionSecondaryCurrencyName)"Energy";
        Assert.Equal("Energy", name.Value);
    }
}

public class ChampionFatigueCounterTests
{
    [Fact]
    public void Constructor_ThrowsOnNegative()
    {
        Assert.Throws<ArgumentException>(() => new ChampionFatigueCounter(-1));
    }

    [Fact]
    public void ImplicitCastToInt_ReturnsValue()
    {
        ChampionFatigueCounter fc = new ChampionFatigueCounter(3);
        int cast = fc;
        Assert.Equal(3, cast);
    }

    [Fact]
    public void ExplicitCastFromInt_CreatesChampionFatigueCounter()
    {
        ChampionFatigueCounter fc = (ChampionFatigueCounter)3;
        Assert.Equal(3, fc.Value);
    }
}

public class ChampionNameTests
{
    [Fact]
    public void Constructor_ThrowsOnEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ChampionName(""));
    }

    [Fact]
    public void Constructor_ThrowsOnWhitespaceOnly()
    {
        Assert.Throws<ArgumentException>(() => new ChampionName("   "));
    }

    [Fact]
    public void ImplicitCastToString_ReturnsValue()
    {
        ChampionName name = new ChampionName("Hero");
        string cast = name;
        Assert.Equal("Hero", cast);
    }

    [Fact]
    public void ExplicitCastFromString_CreatesChampionName()
    {
        ChampionName name = (ChampionName)"Hero";
        Assert.Equal("Hero", name.Value);
    }
}

public class ChampionDescriptionTests
{
    [Fact]
    public void Constructor_HandlesNull_ReturnsEmpty()
    {
        ChampionDescription desc = new ChampionDescription(null!);
        Assert.Equal(string.Empty, desc.Value);
    }

    [Fact]
    public void Constructor_TrimsValue()
    {
        ChampionDescription desc = new ChampionDescription("  text  ");
        Assert.Equal("text", desc.Value);
    }

    [Fact]
    public void ImplicitCastToString_ReturnsValue()
    {
        ChampionDescription desc = new ChampionDescription("lore");
        string cast = desc;
        Assert.Equal("lore", cast);
    }

    [Fact]
    public void ExplicitCastFromString_CreatesDescription()
    {
        ChampionDescription desc = (ChampionDescription)"lore";
        Assert.Equal("lore", desc.Value);
    }
}
