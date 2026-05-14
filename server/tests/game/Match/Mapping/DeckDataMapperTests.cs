using game.Application.Mapping;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.DTO;

namespace game.Tests.Domain.Mapping;

public class DeckDataMapperTests
{
    private static ApiDeckChampionDto MakeApiChampion(int hp = 30, string name = "Hero")
    {
        return new ApiDeckChampionDto
        {
            ChampionID = Guid.NewGuid(),
            Name = name,
            Description = "A great champion",
            HP = hp,
            Picture = "champion.png",
            FactionId = Guid.NewGuid()
        };
    }

    private static ApiDeckCardDto MakeApiCard(
        string name = "Sword",
        int? hp = 3,
        int? attack = 2,
        int cost = 1,
        int cardType = 0,
        List<string>? classes = null)
    {
        return new ApiDeckCardDto
        {
            CardId = Guid.NewGuid(),
            DeckCardId = Guid.NewGuid(),
            CollectionCardId = Guid.NewGuid(),
            Name = name,
            Hp = hp,
            Attack = attack,
            Cost = cost,
            Description = "A weapon",
            Picture = "sword.png",
            CardType = cardType,
            Classes = classes ?? new List<string>()
        };
    }

    [Fact]
    public void Map_ReturnsCorrectDeckId()
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        ApiDeckDataDto api = new ApiDeckDataDto
        {
            Champion = MakeApiChampion(),
            Cards = new List<ApiDeckCardDto>()
        };

        DeckData result = DeckDataMapper.Map(deckId, api);

        Assert.Equal(deckId, result.DeckId);
    }

    [Fact]
    public void Map_MapsCardCount()
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        ApiDeckDataDto api = new ApiDeckDataDto
        {
            Champion = MakeApiChampion(),
            Cards = new List<ApiDeckCardDto> { MakeApiCard(), MakeApiCard("Shield"), MakeApiCard("Axe") }
        };

        DeckData result = DeckDataMapper.Map(deckId, api);

        Assert.Equal(3, result.Cards.Count);
    }

    [Fact]
    public void Map_AssignsSequentialGameCardIds()
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        ApiDeckDataDto api = new ApiDeckDataDto
        {
            Champion = MakeApiChampion(),
            Cards = new List<ApiDeckCardDto> { MakeApiCard(), MakeApiCard("Shield"), MakeApiCard("Axe") }
        };

        DeckData result = DeckDataMapper.Map(deckId, api);

        Assert.Equal(1, result.Cards[0].GameCardId.Value);
        Assert.Equal(2, result.Cards[1].GameCardId.Value);
        Assert.Equal(3, result.Cards[2].GameCardId.Value);
    }

    [Fact]
    public void Map_MapsCardFields()
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        ApiDeckCardDto apiCard = MakeApiCard(name: "Sword", hp: 5, attack: 3, cost: 2, classes: new List<string> { "Warrior" });

        ApiDeckDataDto api = new ApiDeckDataDto
        {
            Champion = MakeApiChampion(),
            Cards = new List<ApiDeckCardDto> { apiCard }
        };

        DeckData result = DeckDataMapper.Map(deckId, api);

        game.Domaine.Match.Entity.GameCardDto card = result.Cards[0];
        Assert.Equal("Sword", card.Name.Value);
        Assert.Equal(5, card.Hp.Value);
        Assert.Equal(3, card.Attack.Value);
        Assert.Equal(3, card.BaseAttack.Value);
        Assert.Equal(2, card.Cost.Value);
        Assert.Equal(2, card.BaseCost.Value);
        Assert.Contains("Warrior", card.Classes.Value);
    }

    [Fact]
    public void Map_SetsCardStateToSleeping()
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        ApiDeckDataDto api = new ApiDeckDataDto
        {
            Champion = MakeApiChampion(),
            Cards = new List<ApiDeckCardDto> { MakeApiCard() }
        };

        DeckData result = DeckDataMapper.Map(deckId, api);

        Assert.Equal(game.Domaine.Match.ValueObject.CardStates.Sleeping.Value, result.Cards[0].States.Value);
    }

    [Fact]
    public void Map_NullHpDefaultsToZero()
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        ApiDeckDataDto api = new ApiDeckDataDto
        {
            Champion = MakeApiChampion(),
            Cards = new List<ApiDeckCardDto> { MakeApiCard(hp: null, attack: null) }
        };

        DeckData result = DeckDataMapper.Map(deckId, api);

        Assert.Equal(0, result.Cards[0].Hp.Value);
        Assert.Equal(0, result.Cards[0].Attack.Value);
    }

    [Fact]
    public void Map_MapsChampionFields()
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        ApiDeckChampionDto apiChamp = MakeApiChampion(hp: 40, name: "Paladin");
        ApiDeckDataDto api = new ApiDeckDataDto
        {
            Champion = apiChamp,
            Cards = new List<ApiDeckCardDto>()
        };

        DeckData result = DeckDataMapper.Map(deckId, api);

        Assert.Equal("Paladin", result.Champion.Name.Value);
        Assert.Equal(40, result.Champion.Hp.Value);
        Assert.Equal(40, result.Champion.BaseHp.Value);
        Assert.Equal(deckId, result.Champion.DeckId);
    }

    [Fact]
    public void Map_ChampionGoldStartsAtZero()
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        ApiDeckDataDto api = new ApiDeckDataDto
        {
            Champion = MakeApiChampion(),
            Cards = new List<ApiDeckCardDto>()
        };

        DeckData result = DeckDataMapper.Map(deckId, api);

        Assert.Equal(0, result.Champion.Gold.Value);
        Assert.Equal(0, result.Champion.BaseGold.Value);
    }

    [Fact]
    public void Map_EmptyCardList_ReturnsEmptyCards()
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        ApiDeckDataDto api = new ApiDeckDataDto
        {
            Champion = MakeApiChampion(),
            Cards = new List<ApiDeckCardDto>()
        };

        DeckData result = DeckDataMapper.Map(deckId, api);

        Assert.Empty(result.Cards);
    }

    [Fact]
    public void ApiDeckCardDto_AllPropertiesCanBeSetAndRead()
    {
        Guid deckCardId = Guid.NewGuid();
        Guid collectionCardId = Guid.NewGuid();
        Guid cardId = Guid.NewGuid();

        ApiDeckCardDto dto = new ApiDeckCardDto
        {
            DeckCardId = deckCardId,
            Quantity = 2,
            CollectionCardId = collectionCardId,
            Rarity = 3,
            CardId = cardId,
            Name = "Lance",
            Hp = 4,
            Attack = 2,
            Cost = 1,
            Description = "A sharp lance",
            Picture = "lance.png",
            Extension = 1,
            CardType = 0,
            Price = 50,
            Classes = new List<string> { "Warrior" }
        };

        Assert.Equal(deckCardId, dto.DeckCardId);
        Assert.Equal(2, dto.Quantity);
        Assert.Equal(collectionCardId, dto.CollectionCardId);
        Assert.Equal(3, dto.Rarity);
        Assert.Equal(cardId, dto.CardId);
        Assert.Equal("Lance", dto.Name);
        Assert.Equal(4, dto.Hp);
        Assert.Equal(2, dto.Attack);
        Assert.Equal(1, dto.Cost);
        Assert.Equal("A sharp lance", dto.Description);
        Assert.Equal("lance.png", dto.Picture);
        Assert.Equal(1, dto.Extension);
        Assert.Equal(0, dto.CardType);
        Assert.Equal(50, dto.Price);
        Assert.Single(dto.Classes);
    }
}
