// =============================================
// FICHIER: Services/GameService.cs
// =============================================
// RÔLE PRINCIPAL:
// Gère toute la logique métier du jeu: validation des actions, application des règles,
// mise à jour de l'état de la partie. Travaille uniquement avec les données en mémoire (Room).
//
// RESPONSABILITÉS:
// 1. Valider les actions des joueurs (PlayCard, Attack, EndTurn, etc.)
// 2. Appliquer les règles du jeu (coûts, positions, tours, etc.)
// 3. Gérer les différents types de cartes (Faction, Equipment, Spell)
// 4. Résoudre les effets des cartes
// 5. Mettre à jour l'état de la partie en temps réel
//
// ARCHITECTURE:
// - Service Singleton (une seule instance pour toutes les parties)
// - Opérations thread-safe (chaque Room a son propre état isolé)
// - Aucun appel à la base de données (tout est chargé en mémoire au départ)
// =============================================

using VortexTCG.Game.Object;

namespace game.Services;

/// <summary>
/// Service de gestion de la logique de jeu.
/// Toutes les opérations sont validées côté serveur pour éviter la triche.
/// </summary>
public class GameService
{
    /// <summary>
    /// Joue une carte depuis la main d'un joueur.
    /// Valide toutes les règles et met à jour l'état de la partie.
    /// </summary>
    /// <param name="gameRoom">État actuel de la partie (en mémoire)</param>
    /// <param name="userId">ID de l'utilisateur qui joue la carte</param>
    /// <param name="cardInstanceId">ID de l'instance de carte dans la main</param>
    /// <param name="position">Position sur le plateau (0-6 pour Faction, -1 pour Spell/Equipment)</param>
    /// <returns>Résultat de l'action (succès ou erreur avec message)</returns>
    public PlayCardResponse PlayCard(Room gameRoom, Guid userId, Guid cardInstanceId, int position)
    {
        // 1️⃣ Identifier le joueur actif
        int playerNumber = DeterminePlayerNumber(gameRoom, userId);
        if (playerNumber == 0)
            return PlayCardResponse.CreateError("Joueur non trouvé dans la partie");

        Player player = playerNumber == 1 ? gameRoom.Player1 : gameRoom.Player2;
        Player opponent = playerNumber == 1 ? gameRoom.Player2 : gameRoom.Player1;

        // 2️⃣ Vérifier que c'est son tour
        if (gameRoom.CurrentPlayer != playerNumber)
            return PlayCardResponse.CreateError("Ce n'est pas votre tour");

        // 4️⃣ Trouver la carte dans la main (toutes les données sont déjà chargées)
        VortexTCG.Game.Object.CardInstance? card = player.Hand.FirstOrDefault(c => c.InstanceId == cardInstanceId);
        if (card == null)
            return PlayCardResponse.CreateError("Carte non trouvée dans votre main");

        // 5️⃣ Vérifier le coût en or
        if (player.Gold < card.Cost)
            return PlayCardResponse.CreateError($"Or insuffisant (besoin: {card.Cost}, disponible: {player.Gold})");

        // 6️⃣ Déléguer selon le type de carte
        PlayCardResponse response = card.Type switch
        {
            CardType.Faction => PlayFactionCard(gameRoom, player, opponent, card, position),
            CardType.Equipment => PlayCardResponse.CreateError("Équipements pas encore implémentés"),
            CardType.Spell => PlayCardResponse.CreateError("Sortilèges pas encore implémentés"),
            _ => PlayCardResponse.CreateError("Type de carte inconnu")
        };

        // 7️⃣ Si succès, retirer la carte de la main et déduire l'or
        if (response.Success)
        {
            player.Hand.Remove(card);
            player.Gold -= card.Cost;
        }

        return response;
    }

    /// <summary>
    /// Joue une carte factionnaire (unité) sur le plateau.
    /// Valide la position et crée l'unité avec summoning sickness.
    /// </summary>
    private PlayCardResponse PlayFactionCard(Room gameRoom, Player player, Player opponent, VortexTCG.Game.Object.CardInstance card, int position)
    {
        // Validation de la position
        if (position == -1)
            return PlayCardResponse.CreateError("Position obligatoire pour une carte factionnaire");

        if (position < 0 || position >= player.Board.Length)
            return PlayCardResponse.CreateError($"Position invalide (doit être entre 0 et {player.Board.Length - 1})");

        if (player.Board[position] != null)
            return PlayCardResponse.CreateError("Cette position est déjà occupée");

        // Créer l'unité sur le plateau
        BoardUnit unit = new BoardUnit
        {
            InstanceId = card.InstanceId,
            CardModelId = card.CardModelId,
            Name = card.Name,
            CurrentAttack = card.Attack ?? 0,
            CurrentDefense = card.Defense ?? 0,
            Position = position,
            IsTapped = false,
            CanAttackThisTurn = false, // Summoning sickness (ne peut pas attaquer ce tour)
            ActiveEffects = new List<string>(card.Effects)
        };

        player.Board[position] = unit;

        // TODO: Résoudre les effets "OnPlay" si présents
        // if (card.Effects.Contains("OnPlay"))
        //     ResolveOnPlayEffects(gameRoom, player, opponent, unit, card);

        return PlayCardResponse.CreateSuccess(
            cardPlayed: card,
            position: position,
            remainingGold: player.Gold - card.Cost,
            message: $"{card.Name} a été invoqué en position {position}"
        );
    }

    /// <summary>
    /// Détermine quel numéro de joueur (1 ou 2) correspond à l'userId donné.
    /// </summary>
    /// <returns>1 pour Player1, 2 pour Player2, 0 si non trouvé</returns>
    private int DeterminePlayerNumber(Room gameRoom, Guid userId)
    {
        if (gameRoom.Player1?.UserId == userId) return 1;
        if (gameRoom.Player2?.UserId == userId) return 2;
        return 0;
    }
}

/// <summary>
/// Réponse après avoir tenté de jouer une carte.
/// Contient le résultat, un message explicatif et les données de la carte jouée.
/// </summary>
public record PlayCardResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public VortexTCG.Game.Object.CardInstance? CardPlayed { get; init; }
    public int? Position { get; init; }
    public int RemainingGold { get; init; }

    /// <summary>Crée une réponse de succès</summary>
    public static PlayCardResponse CreateSuccess(
        VortexTCG.Game.Object.CardInstance cardPlayed,
        int? position,
        int remainingGold,
        string message)
    {
        return new PlayCardResponse
        {
            Success = true,
            Message = message,
            CardPlayed = cardPlayed,
            Position = position,
            RemainingGold = remainingGold
        };
    }

    /// <summary>Crée une réponse d'erreur</summary>
    public static PlayCardResponse CreateError(string message)
    {
        return new PlayCardResponse
        {
            Success = false,
            Message = message
        };
    }
}
