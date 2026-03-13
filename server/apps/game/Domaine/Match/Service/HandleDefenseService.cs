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
    Console.WriteLine("=== TOGGLE DEFENSE ===");
    Console.WriteLine($"Phase actuelle: {match.CurrentPhase.Type}");
    Console.WriteLine($"UserId reçu: {userId.Value}");
    Console.WriteLine($"DefensePosition: {defensePosition}");
    Console.WriteLine($"AttackPosition: {attackPosition}");

    if (match.CurrentPhase.Type != MatchPhaseType.Defense)
    {
        Console.WriteLine("Refus: pas en phase Defense");
    }
    else
    {
        Player currentPlayer = match.GetCurrentPlayer();
        Console.WriteLine($"CurrentPlayer: {currentPlayer.UserId.Value}");

        if (!currentPlayer.UserId.Equals(userId))
        {
            Console.WriteLine("Refus: userId != currentPlayer.UserId");
        }
        else
        {
            GameCardDto? defenseCard = currentPlayer.Board.GetCardAtPosition(defensePosition);

            if (defenseCard == null)
            {
                Console.WriteLine("Refus: aucune carte trouvée à cette position");
            }
            else
            {
                Console.WriteLine($"Carte trouvée: GameCardId={defenseCard.GameCardId}, State={defenseCard.States.Value}");

                if (!CanBeDefendedWith(defenseCard))
                {
                    Console.WriteLine("Refus: carte non défendable (pas Active)");
                }
                else if (!match.AttackHandler.HasAttackAtPosition(attackPosition))
                {
                    Console.WriteLine("Refus: aucune attaque sur cette position");
                }
                else
                {
                    if (match.DefenseHandler.IsDefenseEngagedOnAttack(defensePosition, attackPosition))
                    {
                        Console.WriteLine("Défense déjà engagée -> suppression");
                        match.DefenseHandler.RemoveDefenseByPosition(defensePosition);
                        defenseCard.States = CardStates.Active;
                    }
                    else
                    {
                        Console.WriteLine("Défense ajoutée");
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
    Console.WriteLine($"Nb défenses engagées: {dto.EngagedCards.Count}");

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