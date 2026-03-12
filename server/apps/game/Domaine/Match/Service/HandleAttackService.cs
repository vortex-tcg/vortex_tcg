using System.Threading;
using game.Domaine.Interface;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Event.Action;
using game.Domaine.Match.Interface;
using game.Domaine.Match.ValueObject;

namespace game.Domaine.Match.Service;

public static class HandleAttackService
{
    public static AttackOrderUpdatedDto ToggleAttackCard(
        Agregate.Match match,
        UserId userId,
        int position,
        CancellationToken ct = default)
    {
        if (match.CurrentPhase.Type == MatchPhaseType.Attack)
        {
            Player currentPlayer = match.GetCurrentPlayer();

            if (currentPlayer.UserId.Equals(userId))
            {
                GameCardDto? card = currentPlayer.Board.GetCardAtPosition(position);

                if (card != null && CanBeEngaged(card))
                {
                    if (match.AttackHandler.IsEngaged(position))
                    {
                        match.AttackHandler.RemoveAttackByPosition(position);
                        card.States = CardStates.Active;
                    }
                    else
                    {
                        match.AttackHandler.AddAttack(position, card.GameCardId);
                        card.States = CardStates.Attacking;
                    }
                }
            }
        }

        AttackOrderUpdatedDto dto = match.AttackHandler.FormatAttackResponseDto();

        match.AddEvent(new DomainEvent(
            AttackEvent.ATTACK_ORDER_UPDATED,
            dto
        ));

        return dto;
    }

    private static bool CanBeEngaged(GameCardDto card)
    {
        return card.States.Value == CardStates.Active.Value;
    }
}