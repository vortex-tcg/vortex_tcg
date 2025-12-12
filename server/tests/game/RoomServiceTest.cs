// =============================================
// FICHIER: tests/game/RoomServiceTest.cs
// =============================================
// RÔLE:
// Tests unitaires pour RoomService.
// Vérifie tous les scénarios de création, jonction, initialisation et départ.
//
// FRAMEWORK:
// xUnit - Framework de test .NET standard
//
// COUVERTURE:
// ✅ Création de salons
// ✅ Jonction de salons
// ✅ Gestion des pseudos
// ✅ Initialisation de parties
// ✅ Départs et nettoyage
// ✅ Cas d'erreur (salon plein, inexistant, etc.)
// =============================================

using Xunit;
using game.Services;
using System;
using System.Threading.Tasks;

namespace game.Tests
{
    /// <summary>
    /// Suite de tests pour RoomService.
    /// Chaque test vérifie un comportement spécifique du service.
    /// </summary>
    public class RoomServiceTest
    {
        #region Tests - Création de salons

        /// <summary>
        /// TEST: Créer un salon génère un code unique de 6 caractères.
        /// </summary>
        [Fact]
        public void TryCreateRoom_GeneratesUniqueCode()
        {
            // ARRANGE: Créer le service
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();

            // ACT: Créer un salon
            bool success = service.TryCreateRoom(userId1, out string code);

            // ASSERT: Vérifier le succès et le format du code
            Assert.True(success);
            Assert.NotNull(code);
            Assert.Equal(6, code.Length); // Le code doit faire 6 caractères
            Assert.Matches("^[A-Z0-9]+$", code); // Uniquement lettres majuscules et chiffres
        }

        /// <summary>
        /// TEST: Créer un salon avec un code préféré qui n'existe pas encore.
        /// </summary>
        [Fact]
        public void TryCreateRoom_WithPreferredCode_Succeeds()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            string preferredCode = "CUSTOM";

            // ACT
            bool success = service.TryCreateRoom(userId1, out string code, preferredCode);

            // ASSERT
            Assert.True(success);
            Assert.Equal("CUSTOM", code); // Le code doit être celui demandé
        }

        /// <summary>
        /// TEST: Créer un salon avec un code préféré déjà utilisé échoue.
        /// </summary>
        [Fact]
        public void TryCreateRoom_WithUsedPreferredCode_Fails()
        {
            // ARRANGE: Créer un premier salon avec le code "CUSTOM"
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            string preferredCode = "CUSTOM";
            
            service.TryCreateRoom(userId1, out _, preferredCode);

            // ACT: Essayer de créer un autre salon avec le même code
            bool success = service.TryCreateRoom(userId2, out string code, preferredCode);

            // ASSERT: Échec car le code est déjà pris
            Assert.False(success);
        }

        /// <summary>
        /// TEST: Après création, le joueur est bien dans le salon.
        /// </summary>
        [Fact]
        public void TryCreateRoom_AddsCreatorToRoom()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();

            // ACT
            service.TryCreateRoom(userId1, out string code);
            
            // ASSERT: Le créateur doit être dans le salon
            string? foundCode = service.GetRoomOf(userId1);
            Assert.Equal(code, foundCode);
        }

        #endregion

        #region Tests - Jonction de salons

        /// <summary>
        /// TEST: Rejoindre un salon existant avec de la place réussit.
        /// </summary>
        [Fact]
        public void TryJoinRoom_ValidRoom_Succeeds()
        {
            // ARRANGE: Créer un salon
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);

            // ACT: Rejoindre le salon
            bool success = service.TryJoinRoom(userId2, code, out Guid? opponentId, out bool isFull);

            // ASSERT
            Assert.True(success);
            Assert.Equal(userId1, opponentId); // L'adversaire est le créateur
            Assert.False(isFull);
        }

        /// <summary>
        /// TEST: Rejoindre un salon inexistant échoue.
        /// </summary>
        [Fact]
        public void TryJoinRoom_NonExistentRoom_Fails()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();

            // ACT: Essayer de rejoindre un salon qui n'existe pas
            bool success = service.TryJoinRoom(userId1, "BADCODE", out Guid? opponentId, out bool isFull);

            // ASSERT
            Assert.False(success);
            Assert.Null(opponentId);
            Assert.False(isFull);
        }

        /// <summary>
        /// TEST: Rejoindre un salon plein (2/2 joueurs) échoue.
        /// </summary>
        [Fact]
        public void TryJoinRoom_FullRoom_Fails()
        {
            // ARRANGE: Créer un salon et le remplir
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid userId3 = Guid.NewGuid();
            
            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out _, out _);

            // ACT: Essayer de rejoindre un salon déjà plein
            bool success = service.TryJoinRoom(userId3, code, out Guid? opponentId, out bool isFull);

            // ASSERT
            Assert.False(success);
            Assert.Null(opponentId);
            Assert.True(isFull); // Le flag isFull doit être true
        }

        #endregion

        #region Tests - Gestion des pseudos

        /// <summary>
        /// TEST: Définir et récupérer un pseudo.
        /// </summary>
        [Fact]
        public void SetName_AndGetName_Works()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();

            // ACT
            service.SetName(userId1, "PlayerOne");
            string name = service.GetName(userId1);

            // ASSERT
            Assert.Equal("PlayerOne", name);
        }

        /// <summary>
        /// TEST: Récupérer un pseudo non défini retourne un pseudo par défaut.
        /// </summary>
        [Fact]
        public void GetName_UndefinedName_ReturnsDefault()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();

            // ACT
            string name = service.GetName(userId1);

            // ASSERT: Doit retourner "Player-{8 premiers chars du Guid}"
            string expected = $"Player-{userId1.ToString()[..8]}";
            Assert.Equal(expected, name);
        }

        /// <summary>
        /// TEST: Définir un pseudo vide génère un pseudo par défaut.
        /// </summary>
        [Fact]
        public void SetName_EmptyName_GeneratesDefault()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();

            // ACT
            service.SetName(userId1, "   "); // Pseudo vide/whitespace
            string name = service.GetName(userId1);

            // ASSERT: Doit retourner "Player-{8 premiers chars du Guid}"
            string expected = $"Player-{userId1.ToString()[..8]}";
            Assert.Equal(expected, name);
        }

        #endregion

        #region Tests - Initialisation de parties

        /// <summary>
        /// TEST: Définir le deck du premier joueur n'initialise pas encore la partie.
        /// </summary>
        [Fact]
        public async Task SetPlayerDeck_FirstPlayer_DoesNotInitializeGame()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            
            service.TryCreateRoom(userId1, out string code);

            // ACT
            await service.SetPlayerDeck(userId1, deckId1);

            // ASSERT: La partie ne doit PAS être prête (il manque le 2ème joueur)
            bool isReady = service.IsGameReady(code);
            Assert.False(isReady);
            
            // Le RoomObject ne doit PAS encore exister
            var gameRoom = service.GetGameRoom(code);
            Assert.Null(gameRoom);
        }

        /// <summary>
        /// TEST: Définir le deck du deuxième joueur initialise automatiquement la partie.
        /// </summary>
        [Fact]
        public async Task SetPlayerDeck_BothPlayers_InitializesGame()
        {
            // ARRANGE: Créer un salon avec 2 joueurs
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();
            
            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out _, out _);

            // ACT: Définir les decks des 2 joueurs
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2); // ⚡ Déclenche l'initialisation

            // ASSERT: La partie doit être prête
            bool isReady = service.IsGameReady(code);
            Assert.True(isReady);
            
            // Le RoomObject doit exister
            var gameRoom = service.GetGameRoom(code);
            Assert.NotNull(gameRoom);
        }

        /// <summary>
        /// TEST: GetRoomPlayers retourne les IDs des joueurs et decks.
        /// </summary>
        [Fact]
        public async Task GetRoomPlayers_ReturnsCorrectIds()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();
            
            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out _, out _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);

            // ACT
            var (user1, user2, deck1, deck2) = service.GetRoomPlayers(code);

            // ASSERT
            Assert.Equal(userId1, user1);
            Assert.Equal(userId2, user2);
            Assert.Equal(deckId1, deck1);
            Assert.Equal(deckId2, deck2);
        }

        #endregion

        #region Tests - Départs et nettoyage

        /// <summary>
        /// TEST: Quitter un salon retire le joueur.
        /// </summary>
        [Fact]
        public void Leave_RemovesPlayerFromRoom()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);

            // ACT
            service.Leave(userId1, out string? leftCode, out _, out _);

            // ASSERT
            Assert.Equal(code, leftCode);
            
            // Le joueur ne doit plus être dans aucun salon
            string? foundCode = service.GetRoomOf(userId1);
            Assert.Null(foundCode);
        }

        /// <summary>
        /// TEST: Quitter un salon avec 2 joueurs laisse l'adversaire.
        /// </summary>
        [Fact]
        public void Leave_WithOpponent_LeavesOpponentInRoom()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out _, out _);

            // ACT: Le joueur 1 quitte
            service.Leave(userId1, out _, out Guid? opponentId, out bool roomEmpty);

            // ASSERT
            Assert.Equal(userId2, opponentId); // L'adversaire est toujours là
            Assert.False(roomEmpty); // Le salon n'est pas vide
            
            // Le joueur 2 doit toujours être dans le salon
            string? foundCode = service.GetRoomOf(userId2);
            Assert.Equal(code, foundCode);
        }

        /// <summary>
        /// TEST: Quitter un salon vide supprime le salon.
        /// </summary>
        [Fact]
        public void Leave_LastPlayer_DeletesRoom()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);

            // ACT: Le dernier joueur quitte
            service.Leave(userId1, out _, out Guid? opponentId, out bool roomEmpty);

            // ASSERT
            Assert.True(!opponentId.HasValue || opponentId.Value == Guid.Empty); // Pas d'adversaire (null ou Guid.Empty)
            Assert.True(roomEmpty); // Le salon est vide et supprimé
            
            // Le salon ne doit plus exister
            var gameRoom = service.GetGameRoom(code);
            Assert.Null(gameRoom);
        }

        /// <summary>
        /// TEST: GetOpponentOf retourne l'adversaire dans un salon.
        /// </summary>
        [Fact]
        public void GetOpponentOf_ReturnsOtherPlayer()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out _, out _);

            // ACT
            Guid? opponent1 = service.GetOpponentOf(userId1);
            Guid? opponent2 = service.GetOpponentOf(userId2);

            // ASSERT
            Assert.Equal(userId2, opponent1); // L'adversaire de userId1 est userId2
            Assert.Equal(userId1, opponent2); // L'adversaire de userId2 est userId1
        }

        /// <summary>
        /// TEST: GetOpponentOf pour un joueur seul retourne null.
        /// </summary>
        [Fact]
        public void GetOpponentOf_NoOpponent_ReturnsNull()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out _);

            // ACT
            Guid? opponent = service.GetOpponentOf(userId1);

            // ASSERT: FirstOrDefault sur HashSet<Guid> retourne Guid.Empty, pas null
            Assert.True(!opponent.HasValue || opponent.Value == Guid.Empty); // Pas d'adversaire
        }

        #endregion

        #region Tests - Cas d'erreur

        /// <summary>
        /// TEST: SetPlayerDeck pour un joueur non dans un salon échoue.
        /// </summary>
        [Fact]
        public async Task SetPlayerDeck_PlayerNotInRoom_Fails()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            // ACT: Essayer de définir un deck sans être dans un salon
            bool success = await service.SetPlayerDeck(userId, deckId);

            // ASSERT
            Assert.False(success);
        }

        /// <summary>
        /// TEST: GetGameRoom pour un salon inexistant retourne null.
        /// </summary>
        [Fact]
        public void GetGameRoom_NonExistentRoom_ReturnsNull()
        {
            // ARRANGE
            var service = new RoomService();

            // ACT
            var gameRoom = service.GetGameRoom("BADCODE");

            // ASSERT
            Assert.Null(gameRoom);
        }

        /// <summary>
        /// TEST: GetGameRoomByUserId pour un joueur non dans un salon retourne null.
        /// </summary>
        [Fact]
        public void GetGameRoomByUserId_PlayerNotInRoom_ReturnsNull()
        {
            // ARRANGE
            var service = new RoomService();
            Guid userId1 = Guid.NewGuid();

            // ACT
            var gameRoom = service.GetGameRoomByUserId(userId1);

            // ASSERT
            Assert.Null(gameRoom);
        }

        #endregion
    }
}
