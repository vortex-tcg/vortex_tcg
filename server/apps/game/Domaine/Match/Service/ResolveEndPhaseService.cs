using game.Domaine.Match.Event.Action;

namespace game.Domaine.Match.Service;

using System.Collections.Generic;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;


public static class ResolveEndPhaseService
{
    public static BattleResolveDTOs Apply(Match match)
    {
        Player attackingPlayer = match.GetOpponentPlayer();
        Player defendingPlayer = match.GetCurrentPlayer();

        BattleResolveDTOs result = new BattleResolveDTOs();

        IReadOnlyList<AttackCard> attacks = match.AttackHandler.GetAttackers();

        foreach (AttackCard attack in attacks)
        {
            ResolveSingleAttack(
                match,
                attackingPlayer,
                defendingPlayer,
                attack,
                result
            );
        }

        RemoveDeadCards(attackingPlayer, result);
        RemoveDeadCards(defendingPlayer, result);

        result.CurrentPlayerChampionHp = attackingPlayer.Champion.Hp.Value;
        result.OpponentPlayerChampionHp = defendingPlayer.Champion.Hp.Value;

        match.AttackHandler.ResetAttackHandler();
        match.DefenseHandler.ResetDefenseHandler();

        EndTurnService.Apply(match);

        match.AddEvent(new DomainEvent(
            PhaseEvent.BATTLE_RESOLUTION,
            result
        ));

        return result;
    }

    private static void ResolveSingleAttack(
        Match match,
        Player attackingPlayer,
        Player defendingPlayer,
        AttackCard attack,
        BattleResolveDTOs result)
    {
        GameCardDto? attackerCard = attackingPlayer.Board.GetCardAtPosition(attack.Position);

        if (attackerCard == null)
        {
            return;
        }

        DefenseCard? defense = match.DefenseHandler.GetDefenseByAttackPosition(attack.Position);

        if (defense == null)
        {
            ResolveDirectChampionAttack(
                defendingPlayer,
                attack.Position,
                attackerCard,
                result
            );
            return;
        }

        GameCardDto? defenderCard = defendingPlayer.Board.GetCardAtPosition(defense.Position);

        if (defenderCard == null)
        {
            ResolveDirectChampionAttack(
                defendingPlayer,
                attack.Position,
                attackerCard,
                result
            );
            return;
        }

        ResolveCardBattle(
            attackerCard,
            attack.Position,
            defenderCard,
            defense.Position,
            result
        );
    }

    private static void ResolveCardBattle(
        GameCardDto attackerCard,
        int attackerPosition,
        GameCardDto defenderCard,
        int defenderPosition,
        BattleResolveDTOs result)
    {
        int attackerDamage = attackerCard.Attack.Value;
        int defenderDamage = defenderCard.Attack.Value;

        int attackerCurrentHp = attackerCard.Hp.Value;
        int defenderCurrentHp = defenderCard.Hp.Value;

        int attackerRemainingHp = attackerCurrentHp - defenderDamage;
        int defenderRemainingHp = defenderCurrentHp - attackerDamage;

        attackerCard.Hp = new CardHpValue(attackerRemainingHp);
        defenderCard.Hp = new CardHpValue(defenderRemainingHp);

        bool attackerDied = attackerRemainingHp <= 0;
        bool defenderDied = defenderRemainingHp <= 0;

        CardBattleResultDto battle = new CardBattleResultDto
        {
            AttackerCardId = attackerCard.GameCardId,
            AttackerPosition = attackerPosition,
            DefenderCardId = defenderCard.GameCardId,
            DefenderPosition = defenderPosition,
            DamageToAttacker = defenderDamage,
            DamageToDefender = attackerDamage,
            AttackerRemainingHp = attackerRemainingHp,
            DefenderRemainingHp = defenderRemainingHp,
            AttackerDied = attackerDied,
            DefenderDied = defenderDied
        };

        result.Battles.Add(battle);
    }

    private static void ResolveDirectChampionAttack(
        Player defendingPlayer,
        int attackerPosition,
        GameCardDto attackerCard,
        BattleResolveDTOs result)
    {
        int damage = attackerCard.Attack.Value;
        int championRemainingHp = defendingPlayer.Champion.Hp.Value - damage;

        defendingPlayer.Champion.Hp = new ChampionHp(championRemainingHp);

        DirectChampionDamageDto dto = new DirectChampionDamageDto
        {
            AttackerCardId = attackerCard.GameCardId,
            AttackerPosition = attackerPosition,
            Damage = damage,
            ChampionRemainingHp = championRemainingHp
        };

        result.DirectChampionDamages.Add(dto);
    }

    private static void RemoveDeadCards(Player player, BattleResolveDTOs result)
    {
        List<int> positionsToRemove = new List<int>();

        foreach (KeyValuePair<int, GameCardDto> entry in player.Board.EnumerateSlots())
        {
            int position = entry.Key;
            GameCardDto card = entry.Value;

            if (card.Hp.Value <= 0)
            {
                player.Graveyard.Add(card);
                positionsToRemove.Add(position);
                result.DeadCardIds.Add(card.GameCardId);
            }
        }

        foreach (int position in positionsToRemove)
        {
            player.Board.RemoveCardAtPosition(position);
        }
    }
}