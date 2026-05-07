using System.Threading;
using game.Domaine.Interface;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Event.Action;
using game.Domaine.Match.Interface;
using game.Domaine.Match.ValueObject;

namespace game.Domaine.Match.Service;

public static class HandleDefenseService
{
  public static DefenseUpdatedDto ToggleDefenseCard(
    Agregate.Match match,
    UserId userId,
    int defensePosition,
    int attackPosition,
    CancellationToken ct = default)
{
   
    if (match.CurrentPhase.Type == MatchPhaseType.Defense)
    {

        Player currentPlayer = match.GetCurrentPlayer();

        if (currentPlayer.UserId.Equals(userId))
        {
      
            GameCardDto? defenseCard = currentPlayer.Board.GetCardAtPosition(defensePosition);

            if (defenseCard != null)
            {
                // Explicit removal flow from client (second click on defending card).
                if (attackPosition < 0)
                {
                    if (match.DefenseHandler.IsDefenseEngaged(defensePosition))
                    {
                        match.DefenseHandler.RemoveDefenseByPosition(defensePosition);
                        defenseCard.States = CardStates.Active;
                    }
                }
                else
                {
                    if (!CanBeDefendedWith(defenseCard))
                    {
                        throw new InvalidOperationException("Cette carte ne peut pas défendre pour le moment.");
                    }

                    bool alreadyDefendingThisAttack = match.DefenseHandler.IsDefenseEngagedOnAttack(defensePosition, attackPosition);
                    bool anotherCardAlreadyDefendsThisAttack = match.DefenseHandler.HasDefenseOnAttack(attackPosition) && !alreadyDefendingThisAttack;

                    if (anotherCardAlreadyDefendsThisAttack)
                    {
                        throw new InvalidOperationException("Impossible de défendre cette carte: une autre carte prend déjà les dégâts.");
                    }

                    if (alreadyDefendingThisAttack)
                    {
                        match.DefenseHandler.RemoveDefenseByPosition(defensePosition);
                        defenseCard.States = CardStates.Active;
                    }
                    else
                    {
                        match.DefenseHandler.AddOrReplaceDefense(
                            defensePosition,
                            defenseCard.GameCardId,
                            attackPosition
                        );

                        defenseCard.States = CardStates.Defending;
                    }
                }
            }
        }
    }

    DefenseUpdatedDto dto = match.DefenseHandler.FormatDefenseResponseDto();

    match.AddEvent(new DomainEvent(
        DefenseEvent.DEFENSE_UPDATED,
        dto
    ));

    return dto;
}

    private static bool CanBeDefendedWith(GameCardDto card)
    {
        return card.States.Value == CardStates.Active.Value;
    }
}