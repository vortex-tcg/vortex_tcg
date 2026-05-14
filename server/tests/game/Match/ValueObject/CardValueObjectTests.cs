using game.Domaine.Match.ValueObject;
using Xunit;

namespace game.Tests.Domain.ValueObject;

public class UserIdTests
{
    [Fact]
    public void Constructor_ThrowsOnEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => new UserId(Guid.Empty));
    }

    [Fact]
    public void ImplicitCastToGuid_ReturnsValue()
    {
        Guid g = Guid.NewGuid();
        UserId id = new UserId(g);
        Guid cast = id;
        Assert.Equal(g, cast);
    }

    [Fact]
    public void ExplicitCastFromGuid_CreatesUserId()
    {
        Guid g = Guid.NewGuid();
        UserId id = (UserId)g;
        Assert.Equal(g, (Guid)id);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        Guid g = Guid.NewGuid();
        UserId id = new UserId(g);
        Assert.Equal(g.ToString(), id.ToString());
    }
}

public class DeckIdTests
{
    [Fact]
    public void Constructor_ThrowsOnEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => new DeckId(Guid.Empty));
    }

    [Fact]
    public void ImplicitCastToGuid_ReturnsValue()
    {
        Guid g = Guid.NewGuid();
        DeckId id = new DeckId(g);
        Guid cast = id;
        Assert.Equal(g, cast);
    }

    [Fact]
    public void ExplicitCastFromGuid_CreatesDeckId()
    {
        Guid g = Guid.NewGuid();
        DeckId id = (DeckId)g;
        Assert.Equal(g, (Guid)id);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        Guid g = Guid.NewGuid();
        DeckId id = new DeckId(g);
        Assert.Equal(g.ToString(), id.ToString());
    }
}

public class CardIdTests
{
    [Fact]
    public void Constructor_ThrowsOnEmptyGuid()
    {
        Assert.Throws<ArgumentException>(() => new CardId(Guid.Empty));
    }

    [Fact]
    public void ExplicitCastFromGuid_CreatesCardId()
    {
        Guid g = Guid.NewGuid();
        CardId id = (CardId)g;
        Assert.Equal(g, (Guid)id);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        Guid g = Guid.NewGuid();
        CardId id = new CardId(g);
        Assert.Equal(g.ToString(), id.ToString());
    }
}

public class GameCardIdTests
{
    [Fact]
    public void Constructor_ThrowsOnZero()
    {
        Assert.Throws<ArgumentException>(() => new GameCardId(0));
    }

    [Fact]
    public void Constructor_ThrowsOnNegative()
    {
        Assert.Throws<ArgumentException>(() => new GameCardId(-1));
    }

    [Fact]
    public void ImplicitCastToInt_ReturnsValue()
    {
        GameCardId id = new GameCardId(5);
        int cast = id;
        Assert.Equal(5, cast);
    }

    [Fact]
    public void ExplicitCastFromInt_CreatesGameCardId()
    {
        GameCardId id = (GameCardId)3;
        Assert.Equal(3, id.Value);
    }

    [Fact]
    public void ToString_ReturnsIntString()
    {
        GameCardId id = new GameCardId(7);
        Assert.Equal("7", id.ToString());
    }
}

public class CardNameTests
{
    [Fact]
    public void Constructor_ThrowsOnEmpty()
    {
        Assert.Throws<ArgumentException>(() => new CardName(""));
    }

    [Fact]
    public void Constructor_ThrowsOnWhitespace()
    {
        Assert.Throws<ArgumentException>(() => new CardName("   "));
    }

    [Fact]
    public void Constructor_TrimsValue()
    {
        CardName name = new CardName("  Sword  ");
        Assert.Equal("Sword", name.Value);
    }

    [Fact]
    public void ImplicitCastToString_ReturnsValue()
    {
        CardName name = new CardName("Axe");
        string cast = name;
        Assert.Equal("Axe", cast);
    }

    [Fact]
    public void ExplicitCastFromString_CreatesCardName()
    {
        CardName name = (CardName)"Shield";
        Assert.Equal("Shield", name.Value);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        CardName name = new CardName("Lance");
        Assert.Equal("Lance", name.ToString());
    }
}

public class CardHpValueTests
{
    [Fact]
    public void ImplicitCastToInt_ReturnsValue()
    {
        CardHpValue hp = new CardHpValue(10);
        int cast = hp;
        Assert.Equal(10, cast);
    }

    [Fact]
    public void ExplicitCastFromInt_CreatesCardHpValue()
    {
        CardHpValue hp = (CardHpValue)5;
        Assert.Equal(5, hp.Value);
    }

    [Fact]
    public void ToString_ReturnsIntString()
    {
        CardHpValue hp = new CardHpValue(3);
        Assert.Equal("3", hp.ToString());
    }
}

public class CardAttackValueTests
{
    [Fact]
    public void Constructor_ThrowsOnNegative()
    {
        Assert.Throws<ArgumentException>(() => new CardAttackValue(-1));
    }

    [Fact]
    public void ImplicitCastToInt_ReturnsValue()
    {
        CardAttackValue atk = new CardAttackValue(4);
        int cast = atk;
        Assert.Equal(4, cast);
    }

    [Fact]
    public void ExplicitCastFromInt_CreatesCardAttackValue()
    {
        CardAttackValue atk = (CardAttackValue)2;
        Assert.Equal(2, atk.Value);
    }

    [Fact]
    public void ToString_ReturnsIntString()
    {
        CardAttackValue atk = new CardAttackValue(3);
        Assert.Equal("3", atk.ToString());
    }
}

public class CardCostValueTests
{
    [Fact]
    public void Constructor_ThrowsOnNegative()
    {
        Assert.Throws<ArgumentException>(() => new CardCostValue(-1));
    }

    [Fact]
    public void ImplicitCastToInt_ReturnsValue()
    {
        CardCostValue cost = new CardCostValue(3);
        int cast = cost;
        Assert.Equal(3, cast);
    }

    [Fact]
    public void ExplicitCastFromInt_CreatesCardCostValue()
    {
        CardCostValue cost = (CardCostValue)1;
        Assert.Equal(1, cost.Value);
    }

    [Fact]
    public void ToString_ReturnsIntString()
    {
        CardCostValue cost = new CardCostValue(2);
        Assert.Equal("2", cost.ToString());
    }
}

public class CardDescriptionTests
{
    [Fact]
    public void Constructor_HandlesNull_ReturnsEmpty()
    {
        CardDescription desc = new CardDescription(null!);
        Assert.Equal(string.Empty, desc.Value);
    }

    [Fact]
    public void Constructor_TrimsValue()
    {
        CardDescription desc = new CardDescription("  text  ");
        Assert.Equal("text", desc.Value);
    }

    [Fact]
    public void ImplicitCastToString_ReturnsValue()
    {
        CardDescription desc = new CardDescription("hello");
        string cast = desc;
        Assert.Equal("hello", cast);
    }

    [Fact]
    public void ExplicitCastFromString_CreatesDescription()
    {
        CardDescription desc = (CardDescription)"hello";
        Assert.Equal("hello", desc.Value);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        CardDescription desc = new CardDescription("abc");
        Assert.Equal("abc", desc.ToString());
    }
}

public class CardImageUrlTests
{
    [Fact]
    public void Constructor_HandlesNull_ReturnsEmpty()
    {
        CardImageUrl url = new CardImageUrl(null!);
        Assert.Equal(string.Empty, url.Value);
    }

    [Fact]
    public void ImplicitCastToString_ReturnsValue()
    {
        CardImageUrl url = new CardImageUrl("img.png");
        string cast = url;
        Assert.Equal("img.png", cast);
    }

    [Fact]
    public void ExplicitCastFromString_CreatesUrl()
    {
        CardImageUrl url = (CardImageUrl)"img.png";
        Assert.Equal("img.png", url.Value);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        CardImageUrl url = new CardImageUrl("pic.png");
        Assert.Equal("pic.png", url.ToString());
    }
}

public class CardClassesTests
{
    [Fact]
    public void Constructor_HandlesNull_ReturnsEmpty()
    {
        CardClasses cc = new CardClasses(null!);
        Assert.Empty(cc.Value);
    }

    [Fact]
    public void ToString_FormatsAsList()
    {
        CardClasses cc = new CardClasses(new[] { "Warrior", "Mage" });
        Assert.Equal("[Warrior,Mage]", cc.ToString());
    }
}

public class CardStatesTests
{
    [Fact]
    public void Constructor_WithIntRaw_MapsToEnum()
    {
        CardStates state = new CardStates(1);
        Assert.Equal(CardState.Active, state.Value);
    }

    [Fact]
    public void StaticSleeping_HasCorrectValue()
    {
        Assert.Equal(CardState.Sleeping, CardStates.Sleeping.Value);
    }

    [Fact]
    public void ToString_ReturnsEnumName()
    {
        Assert.Equal("Active", CardStates.Active.ToString());
    }
}
