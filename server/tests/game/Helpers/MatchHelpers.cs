using game.Domaine.Match.Entity;
using game.Domaine.Match.Interface;
using game.Domaine.Match.ValueObject;
using MatchAggregate = game.Domaine.Match.Agregate.Match;
using VCardState = game.Domaine.Match.ValueObject.CardState;

namespace game.Tests.Helpers;

public static class MatchHelpers
{
    public static GameCardDto MakeCard(
        int gameCardId,
        int hp = 3,
        int attack = 2,
        int cost = 1,
        VCardState state = VCardState.Active)
    {
        return new GameCardDto
        {
            Id = new CardId(Guid.NewGuid()),
            GameCardId = new GameCardId(gameCardId),
            Name = new CardName($"Card{gameCardId}"),
            Hp = new CardHpValue(hp),
            Attack = new CardAttackValue(attack),
            BaseAttack = new CardAttackValue(attack),
            Cost = new CardCostValue(cost),
            BaseCost = new CardCostValue(cost),
            Description = new CardDescription(""),
            CardType = CardType.GUARD,
            Classes = new CardClasses(Array.Empty<string>()),
            States = new CardStates(state),
            ImageUrl = new CardImageUrl("")
        };
    }

    public static GameChampionDto MakeChampion(int hp = 30, int gold = 5, DeckId? deckId = null)
    {
        DeckId did = deckId ?? new DeckId(Guid.NewGuid());
        return new GameChampionDto
        {
            Id = new ChampionId(Guid.NewGuid()),
            DeckId = did,
            BaseHp = new ChampionBaseHp(hp),
            Hp = new ChampionHp(hp),
            BaseGold = new ChampionBaseGold(gold),
            Gold = new ChampionGold(gold),
            SecondaryCurrency = new ChampionSecondaryCurrency(0),
            SecondaryCurrencyName = new ChampionSecondaryCurrencyName("Mana"),
            FatigueCounter = new ChampionFatigueCounter(0),
            Name = new ChampionName("Hero"),
            Description = new ChampionDescription("")
        };
    }

    public static Player MakePlayer(
        UserId? userId = null,
        IEnumerable<GameCardDto>? deck = null,
        GameChampionDto? champion = null)
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        GameChampionDto champ = champion ?? MakeChampion(deckId: deckId);
        return new Player(
            userId ?? new UserId(Guid.NewGuid()),
            deckId,
            new PlayerDeck(deck ?? Array.Empty<GameCardDto>()),
            champ
        );
    }

    public static MatchAggregate MakeMatch(
        Player? p1 = null,
        Player? p2 = null,
        IPhase? phase = null)
    {
        return new MatchAggregate(
            p1 ?? MakePlayer(),
            p2 ?? MakePlayer(),
            phase ?? new StandByPhase()
        );
    }

    public static MatchAggregate MakeMatchInPhase<TPhase>(Player? p1 = null, Player? p2 = null)
        where TPhase : IPhase, new()
    {
        return new MatchAggregate(
            p1 ?? MakePlayer(),
            p2 ?? MakePlayer(),
            new TPhase()
        );
    }
}
