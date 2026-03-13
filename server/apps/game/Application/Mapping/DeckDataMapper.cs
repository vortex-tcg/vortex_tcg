using game.Infrastructure.DTO;
using game.Domaine.Match.Entity; 
using game.Domaine.Match.Agregate;
using game.Domaine.Match.ValueObject;

namespace game.Application.Mapping;

public static class DeckDataMapper
{
    public static DeckData Map(DeckId deckId, ApiDeckDataDto api)
    {
        List<GameCardDto> cards = new List<GameCardDto>(api.Cards.Count);

        int runningGameCardId = 1;

        foreach (ApiDeckCardDto c in api.Cards)
        {
            int hp = c.Hp ?? 0;
            int atk = c.Attack ?? 0;

            GameCardDto dto = new GameCardDto
            {
                Id = (CardId)c.CardId,
                GameCardId = (GameCardId)runningGameCardId++,

                Name = (CardName)c.Name,
                Hp = (CardHpValue)hp,
                Attack = (CardAttackValue)atk,
                BaseAttack = (CardAttackValue)atk,

                Cost = (CardCostValue)c.Cost,
                BaseCost = (CardCostValue)c.Cost,

                Description = (CardDescription)c.Description,
                CardType = (CardType)c.CardType,

                Classes = new CardClasses(c.Classes),
                States = CardStates.Sleeping,
                ImageUrl = (CardImageUrl)c.Picture
            };

            cards.Add(dto);
        }

        ApiDeckChampionDto ch = api.Champion;

        GameChampionDto champ = new GameChampionDto
        {
            Id = (ChampionId)ch.ChampionID,
            DeckId = deckId,
            Name = (ChampionName)ch.Name,
            BaseHp = (ChampionBaseHp)ch.HP,
            Hp = (ChampionHp)ch.HP,
            Description = (ChampionDescription)ch.Description,
            BaseGold = (ChampionBaseGold)0,
            Gold = (ChampionGold)0,
            SecondaryCurrency = (ChampionSecondaryCurrency)0,
            SecondaryCurrencyName = (ChampionSecondaryCurrencyName)"",
            FatigueCounter = (ChampionFatigueCounter)0
        };

        return new DeckData
        {
            DeckId = deckId,
            Cards = cards,
            Champion = champ
        };
    }
}
