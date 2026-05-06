using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;
using MatchAggregate = game.Domaine.Match.Agregate.Match;
using VCardState = game.Domaine.Match.ValueObject.CardState;

namespace game.Tests.Domain.Service;

public class ResolveEndPhaseServiceTests
{
    private static MatchAggregate BuildMatchWithCards(
        GameCardDto? attackerCard = null,
        GameCardDto? defenderCard = null,
        int attackerHp = 30,
        int defenderHp = 30)
    {
        GameChampionDto attackerChampion = MatchHelpers.MakeChampion(hp: attackerHp);
        GameChampionDto defenderChampion = MatchHelpers.MakeChampion(hp: defenderHp);

        Player attacker = MatchHelpers.MakePlayer(champion: attackerChampion);
        Player defender = MatchHelpers.MakePlayer(champion: defenderChampion);

        if (attackerCard != null)
            attacker.Board.Place(1, attackerCard);

        if (defenderCard != null)
            defender.Board.Place(2, defenderCard);

        MatchAggregate match = new MatchAggregate(attacker, defender, new DefensePhase());
        match.SetCurrentPlayerPosition(2);
        return match;
    }

    [Fact]
    public void Apply_CardVsCard_DealsCorrectDamage()
    {
        GameCardDto attCard = MatchHelpers.MakeCard(1, hp: 5, attack: 3, state: VCardState.Attacking);
        GameCardDto defCard = MatchHelpers.MakeCard(2, hp: 4, attack: 2, state: VCardState.Defending);

        MatchAggregate match = BuildMatchWithCards(attCard, defCard);
        match.AttackHandler.AddAttack(1, 1);
        match.DefenseHandler.AddOrReplaceDefense(defensePosition: 2, gameCardId: 2, attackPosition: 1);

        BattleResolveDTOs result = ResolveEndPhaseService.Apply(match);

        Assert.Single(result.Battles);
        CardBattleResultDto battle = result.Battles[0];
        Assert.Equal(3, battle.DamageToDefender);
        Assert.Equal(2, battle.DamageToAttacker);
        Assert.Equal(3, battle.AttackerRemainingHp);
        Assert.Equal(1, battle.DefenderRemainingHp);
        Assert.False(battle.AttackerDied);
        Assert.False(battle.DefenderDied);
    }

    [Fact]
    public void Apply_CardVsCard_DefenderDies()
    {
        GameCardDto attCard = MatchHelpers.MakeCard(1, hp: 5, attack: 5);
        GameCardDto defCard = MatchHelpers.MakeCard(2, hp: 3, attack: 1);

        MatchAggregate match = BuildMatchWithCards(attCard, defCard);
        match.AttackHandler.AddAttack(1, 1);
        match.DefenseHandler.AddOrReplaceDefense(defensePosition: 2, gameCardId: 2, attackPosition: 1);

        BattleResolveDTOs result = ResolveEndPhaseService.Apply(match);

        Assert.Single(result.Battles);
        Assert.True(result.Battles[0].DefenderDied);
        Assert.Contains(2, result.DeadCardIds);
        Assert.Equal(0, match.Player2.Board.Count);
        Assert.Equal(1, match.Player2.Graveyard.Count);
    }

    [Fact]
    public void Apply_CardVsCard_AttackerDies()
    {
        GameCardDto attCard = MatchHelpers.MakeCard(1, hp: 2, attack: 1);
        GameCardDto defCard = MatchHelpers.MakeCard(2, hp: 5, attack: 5);

        MatchAggregate match = BuildMatchWithCards(attCard, defCard);
        match.AttackHandler.AddAttack(1, 1);
        match.DefenseHandler.AddOrReplaceDefense(defensePosition: 2, gameCardId: 2, attackPosition: 1);

        BattleResolveDTOs result = ResolveEndPhaseService.Apply(match);

        Assert.True(result.Battles[0].AttackerDied);
        Assert.Contains(1, result.DeadCardIds);
        Assert.Equal(0, match.Player1.Board.Count);
    }

    [Fact]
    public void Apply_DirectChampionAttack_WhenNoDefender()
    {
        GameCardDto attCard = MatchHelpers.MakeCard(1, hp: 5, attack: 4);

        MatchAggregate match = BuildMatchWithCards(attackerCard: attCard, defenderCard: null, defenderHp: 30);
        match.AttackHandler.AddAttack(1, 1);

        BattleResolveDTOs result = ResolveEndPhaseService.Apply(match);

        Assert.Empty(result.Battles);
        Assert.Single(result.DirectChampionDamages);
        Assert.Equal(4, result.DirectChampionDamages[0].Damage);
        Assert.Equal(26, result.DirectChampionDamages[0].ChampionRemainingHp);
        Assert.Equal(26, match.Player2.Champion.Hp.Value);
    }

    [Fact]
    public void Apply_ResetsHandlersAfterResolution()
    {
        GameCardDto attCard = MatchHelpers.MakeCard(1, hp: 5, attack: 3);

        MatchAggregate match = BuildMatchWithCards(attackerCard: attCard, defenderCard: null);
        match.AttackHandler.AddAttack(1, 1);

        ResolveEndPhaseService.Apply(match);

        Assert.Empty(match.AttackHandler.GetAttackers());
        Assert.Empty(match.DefenseHandler.GetDefenders());
    }

    [Fact]
    public void Apply_FallbackAttack_UsesCardsInAttackingState()
    {
        GameCardDto attCard = MatchHelpers.MakeCard(1, hp: 5, attack: 3, state: VCardState.Attacking);

        MatchAggregate match = BuildMatchWithCards(attackerCard: attCard, defenderCard: null, defenderHp: 30);

        BattleResolveDTOs result = ResolveEndPhaseService.Apply(match);

        Assert.Single(result.DirectChampionDamages);
        Assert.Equal(3, result.DirectChampionDamages[0].Damage);
    }

    [Fact]
    public void Apply_ChampionDies_AddsMatchEndedEvent()
    {
        GameCardDto attCard = MatchHelpers.MakeCard(1, hp: 5, attack: 30);

        MatchAggregate match = BuildMatchWithCards(attackerCard: attCard, defenderCard: null, defenderHp: 1);
        match.AttackHandler.AddAttack(1, 1);

        match.PullEvents();
        ResolveEndPhaseService.Apply(match);

        IReadOnlyList<game.Domaine.Interface.IEvent> events = match.PullEvents();
        bool hasMatchEnded = events.OfType<DomainEvent>()
            .Any(e => e.Name == MatchEvent.MATCH_ENDED);
        Assert.True(hasMatchEnded);
    }
}
