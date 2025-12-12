// =============================================
// FICHIER: tests/game/ChangePhaseTest.cs
// =============================================
// RÔLE:
// Tests unitaires pour la gestion des phases de jeu (Room.ChangePhase).
// Vérifie tous les scénarios de transitions de phases et validations.
//
// FRAMEWORK:
// xUnit - Framework de test .NET standard (même que RoomServiceTest.cs)
//
// COUVERTURE:
// ✅ Transitions normales de phases
// ✅ Validation des joueurs (seul le joueur actuel peut changer)
// ✅ Règles spéciales (Placement = manuel uniquement)
// ✅ Alternance des joueurs (Player 1 ↔ Player 2)
// ✅ Timeout et règles serveur
// ✅ Cas d'erreur et edge cases
// =============================================

using Xunit;
using VortexTCG.Game.Object;
using System;
using System.Threading.Tasks;

namespace game.Tests
{
    /// <summary>
    /// Suite de tests pour la gestion des phases de jeu.
    /// Chaque test vérifie un comportement spécifique de Room.ChangePhase().
    /// ChangePhase retourne maintenant ChangePhaseResult au lieu de bool.
    /// </summary>
    public class ChangePhaseTest
    {
        #region Helper Methods

        /// <summary>
        /// Crée une Room configurée avec 2 joueurs pour les tests.
        /// </summary>
        private async Task<(Room room, Guid player1Id, Guid player2Id)> CreateConfiguredRoom()
        {
            // Créer des IDs de test
            Guid player1Id = Guid.NewGuid();
            Guid player2Id = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            // Créer et initialiser une room
            Room room = new Room();
            await room.setUser1(player1Id, deckId);
            await room.setUser2(player2Id, deckId);

            return (room, player1Id, player2Id);
        }

        #endregion

        #region Tests - Transitions Normales de Phases

        /// <summary>
        /// TEST: Transition complète Draw → Placement → Attack → Defense → EndTurn.
        /// Vérifie que toutes les phases s'enchaînent correctement.
        /// </summary>
        [Fact]
        public async Task ChangePhase_NormalFlow_TransitionsCorrectly()
        {
            // ARRANGE: Créer une room configurée
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();

            // ACT & ASSERT: Draw → Placement
            var result1 = room.ChangePhase(player1Id, isManual: true);
            Assert.Equal(ChangePhaseResult.SUCCESS, result1);
            string phaseInfo1 = room.GetPhaseInfo();
            Assert.Contains("Placement", phaseInfo1);
            Assert.Contains("Joueur 1", phaseInfo1);

            // ACT & ASSERT: Placement → Attack
            var result2 = room.ChangePhase(player1Id, isManual: true);
            Assert.Equal(ChangePhaseResult.SUCCESS, result2);
            string phaseInfo2 = room.GetPhaseInfo();
            Assert.Contains("Attack", phaseInfo2);
            Assert.Contains("Joueur 1", phaseInfo2);

            // ACT & ASSERT: Attack → Defense (switches to Player 2)
            var result3 = room.ChangePhase(player1Id, isManual: true);
            Assert.Equal(ChangePhaseResult.SUCCESS, result3);
            string phaseInfo3 = room.GetPhaseInfo();
            Assert.Contains("Defense", phaseInfo3);
            Assert.Contains("Joueur 2", phaseInfo3); // Player switched during attack

            // ACT & ASSERT: Defense → EndTurn (still Player 2)
            var result4 = room.ChangePhase(player2Id, isManual: true); // Now player2 controls
            Assert.Equal(ChangePhaseResult.SUCCESS, result4);
            string phaseInfo4 = room.GetPhaseInfo();
            Assert.Contains("EndTurn", phaseInfo4);
            Assert.Contains("Joueur 2", phaseInfo4);

            // ACT & ASSERT: EndTurn → Draw (restarts cycle for Player 2)
            var result5 = room.ChangePhase(player2Id, isManual: true);
            Assert.Equal(ChangePhaseResult.PHASECOMPLETED, result5);
            string phaseInfo5 = room.GetPhaseInfo();
            Assert.Contains("Draw", phaseInfo5);
            Assert.Contains("Joueur 2", phaseInfo5); // Same player, new round
        }

        /// <summary>
        /// TEST: GetPhaseInfo retourne des informations cohérentes.
        /// Vérifie que les informations essentielles sont présentes.
        /// </summary>
        [Fact]
        public async Task GetPhaseInfo_ReturnsValidInformation()
        {
            // ARRANGE
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();

            // ACT
            string info = room.GetPhaseInfo();

            // ASSERT: Vérifier que les informations essentielles sont présentes
            Assert.Contains("Joueur", info);
            Assert.Contains("Phase", info);
            Assert.Contains("Temps", info);
        }

        #endregion

        #region Tests - Alternance des Joueurs

        /// <summary>
        /// TEST: Alternance correcte entre les joueurs.
        /// Vérifie que Player 1 → Player 2 → Player 1, etc.
        /// </summary>
        [Fact]
        public async Task ChangePhase_PlayerAlternation_SwitchesPlayersCorrectly()
        {
            // ARRANGE: Créer une room configurée
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();

            // ACT: Player 1's turn until attack (which switches player)
            room.ChangePhase(player1Id, isManual: true); // Draw → Placement
            room.ChangePhase(player1Id, isManual: true); // Placement → Attack
            room.ChangePhase(player1Id, isManual: true); // Attack → Defense (switches to Player 2)

            // ASSERT: Now it's Player 2's turn (defense phase)
            string infoPlayer2Defense = room.GetPhaseInfo();
            Assert.Contains("Joueur 2", infoPlayer2Defense);
            Assert.Contains("Defense", infoPlayer2Defense);

            // ACT: Player 2 continues their phases
            room.ChangePhase(player2Id, isManual: true); // Defense → EndTurn
            room.ChangePhase(player2Id, isManual: true); // EndTurn → Draw (Player 2 new round)

            // ASSERT: Still Player 2, new round
            string infoPlayer2Draw = room.GetPhaseInfo();
            Assert.Contains("Joueur 2", infoPlayer2Draw);
            Assert.Contains("Draw", infoPlayer2Draw);

            // ACT: Player 2's full turn
            room.ChangePhase(player2Id, isManual: true); // Draw → Placement
            room.ChangePhase(player2Id, isManual: true); // Placement → Attack
            room.ChangePhase(player2Id, isManual: true); // Attack → Defense (switches to Player 1)

            // ASSERT: Back to Player 1 (defense phase)
            string backToPlayer1 = room.GetPhaseInfo();
            Assert.Contains("Joueur 1", backToPlayer1);
            Assert.Contains("Defense", backToPlayer1);
        }

        #endregion

        #region Tests - Validation des Joueurs

        /// <summary>
        /// TEST: Seul le joueur actuel peut changer de phase.
        /// Les autres joueurs doivent être refusés.
        /// </summary>
        [Fact]
        public async Task ChangePhase_WrongPlayer_ReturnsWrongPlayer()
        {
            // ARRANGE: C'est le tour du Player 1 (début de partie)
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();

            // ACT: Player 2 essaie de changer la phase (pas son tour)
            var result = room.ChangePhase(player2Id, isManual: true);

            // ASSERT: Doit être refusé avec WRONGPLAYER
            Assert.Equal(ChangePhaseResult.WRONGPLAYER, result);

            // Vérifier que la phase n'a pas changé
            string phaseInfo = room.GetPhaseInfo();
            Assert.Contains("Draw", phaseInfo);
            Assert.Contains("Joueur 1", phaseInfo);
        }

        /// <summary>
        /// TEST: Validation avec un joueur inexistant.
        /// </summary>
        [Fact]
        public async Task ChangePhase_NonExistentPlayer_ReturnsWrongPlayer()
        {
            // ARRANGE
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();
            Guid fakePlayerId = Guid.NewGuid(); // Joueur qui n'existe pas dans la room

            // ACT
            var result = room.ChangePhase(fakePlayerId, isManual: true);

            // ASSERT
            Assert.Equal(ChangePhaseResult.WRONGPLAYER, result);
        }

        #endregion

        #region Tests - Règles Spéciales Phase Placement

        /// <summary>
        /// TEST: La phase Placement ne peut être changée QUE manuellement.
        /// Les appels automatiques (serveur) doivent être refusés.
        /// </summary>
        [Fact]
        public async Task ChangePhase_PlacementPhaseAutomatic_ReturnsFailedManualRequired()
        {
            // ARRANGE: Aller jusqu'à la phase Placement
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();
            room.ChangePhase(player1Id, isManual: true); // Draw → Placement

            // Vérifier qu'on est bien en Placement
            Assert.Contains("Placement", room.GetPhaseInfo());

            // ACT: Essayer de changer automatiquement (serveur)
            var automaticResult = room.ChangePhase(player1Id, isManual: false);

            // ASSERT: Doit être refusé avec FAILEDMANUALREQUIRED
            Assert.Equal(ChangePhaseResult.FAILEDMANUALREQUIRED, automaticResult);

            // Vérifier qu'on est toujours en Placement
            Assert.Contains("Placement", room.GetPhaseInfo());

            // ACT: Changer manuellement (joueur)
            var manualResult = room.ChangePhase(player1Id, isManual: true);

            // ASSERT: Doit fonctionner
            Assert.Equal(ChangePhaseResult.SUCCESS, manualResult);
            Assert.Contains("Attack", room.GetPhaseInfo());
        }

        /// <summary>
        /// TEST: Toutes les autres phases peuvent être changées automatiquement.
        /// </summary>
        [Fact]
        public async Task ChangePhase_NonPlacementPhasesAutomatic_Succeeds()
        {
            // ARRANGE: Aller jusqu'à la phase Attack
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();
            room.ChangePhase(player1Id, isManual: true); // Draw → Placement
            room.ChangePhase(player1Id, isManual: true); // Placement → Attack

            // ACT & ASSERT: Attack phase - changement automatique (switches to player 2)
            var attackResult = room.ChangePhase(player1Id, isManual: false);
            Assert.Equal(ChangePhaseResult.SUCCESS, attackResult);
            Assert.Contains("Defense", room.GetPhaseInfo());
            Assert.Contains("Joueur 2", room.GetPhaseInfo()); // Player switched

            // ACT & ASSERT: Defense phase - changement automatique (player 2 now)
            var defenseResult = room.ChangePhase(player2Id, isManual: false);
            Assert.Equal(ChangePhaseResult.SUCCESS, defenseResult);
            Assert.Contains("EndTurn", room.GetPhaseInfo());
            Assert.Contains("Joueur 2", room.GetPhaseInfo());

            // ACT & ASSERT: EndTurn phase - changement automatique (restarts for player 2)
            var endTurnResult = room.ChangePhase(player2Id, isManual: false);
            Assert.Equal(ChangePhaseResult.PHASECOMPLETED, endTurnResult);
            Assert.Contains("Draw", room.GetPhaseInfo());
            Assert.Contains("Joueur 2", room.GetPhaseInfo()); // Same player, new round
        }

        #endregion

        #region Tests - Timeout et Règles Serveur

        /// <summary>
        /// TEST: IsPhaseExpired retourne une valeur boolean valide.
        /// Note: Test de base car attendre 1 minute réelle serait trop long.
        /// </summary>
        [Fact]
        public async Task IsPhaseExpired_ReturnsValidBoolean()
        {
            // ARRANGE
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();

            // ACT
            bool isExpired = room.IsPhaseExpired();

            // ASSERT: Test que la méthode existe et retourne une valeur bool
            Assert.False(isExpired); // Phase should not be expired immediately after start
        }

        /// <summary>
        /// TEST: CanServerChangePhase selon les règles.
        /// - Placement: jamais
        /// - Autres phases: selon timeout/actions possibles
        /// </summary>
        [Fact]
        public async Task CanServerChangePhase_PlacementPhase_ReturnsFalse()
        {
            // ARRANGE: Aller en phase Placement
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();
            room.ChangePhase(player1Id, isManual: true); // Draw → Placement

            // ACT
            bool canChange = room.CanServerChangePhase();

            // ASSERT
            Assert.False(canChange);
        }

        /// <summary>
        /// TEST: CanServerChangePhase pour les autres phases.
        /// </summary>
        [Fact]
        public async Task CanServerChangePhase_NonPlacementPhase_ChecksTimeout()
        {
            // ARRANGE: Aller en phase Attack
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();
            room.ChangePhase(player1Id, isManual: true); // Draw → Placement
            room.ChangePhase(player1Id, isManual: true); // Placement → Attack

            // ACT
            bool canChange = room.CanServerChangePhase();

            // ASSERT: Pour les phases non-Placement, retourne selon le timeout
            // (false car la phase vient juste de commencer)
            Assert.False(canChange);
        }

        #endregion

        #region Tests - Edge Cases et Robustesse

        /// <summary>
        /// TEST: Appels multiples consécutifs sur la même phase.
        /// </summary>
        [Fact]
        public async Task ChangePhase_MultipleCallsSamePhase_AdvancesCorrectly()
        {
            // ARRANGE
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();

            // ACT: Plusieurs appels consécutifs
            var result1 = room.ChangePhase(player1Id, isManual: true); // Draw → Placement
            var result2 = room.ChangePhase(player1Id, isManual: true); // Placement → Attack
            var result3 = room.ChangePhase(player1Id, isManual: true); // Attack → Defense (switches to player 2)

            // ASSERT: Tous doivent fonctionner
            Assert.Equal(ChangePhaseResult.SUCCESS, result1);
            Assert.Equal(ChangePhaseResult.SUCCESS, result2);
            Assert.Equal(ChangePhaseResult.SUCCESS, result3);

            // Vérifier l'état final (now player 2 in defense phase)
            Assert.Contains("Defense", room.GetPhaseInfo());
            Assert.Contains("Joueur 2", room.GetPhaseInfo());
        }

        /// <summary>
        /// TEST: Cycle complet de plusieurs tours.
        /// Vérifie que le système fonctionne sur plusieurs tours consécutifs.
        /// </summary>
        [Fact]
        public async Task ChangePhase_MultipleTurns_MaintainsCorrectState()
        {
            // ARRANGE
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();

            // ACT: Player 1's phases until attack (switches to player 2)
            room.ChangePhase(player1Id, isManual: true); // Draw → Placement
            room.ChangePhase(player1Id, isManual: true); // Placement → Attack
            room.ChangePhase(player1Id, isManual: true); // Attack → Defense (switch to Player 2)

            // Player 2 continues from defense
            room.ChangePhase(player2Id, isManual: true); // Defense → EndTurn
            room.ChangePhase(player2Id, isManual: true); // EndTurn → Draw (Player 2's new round)

            // Player 2's full turn
            room.ChangePhase(player2Id, isManual: true); // Draw → Placement
            room.ChangePhase(player2Id, isManual: true); // Placement → Attack
            room.ChangePhase(player2Id, isManual: true); // Attack → Defense (switch to Player 1)

            // Player 1 continues from defense
            room.ChangePhase(player1Id, isManual: true); // Defense → EndTurn
            room.ChangePhase(player1Id, isManual: true); // EndTurn → Draw (Player 1's new round)

            // Player 1's new turn
            var result = room.ChangePhase(player1Id, isManual: true); // Draw → Placement

            // ASSERT: Le système doit toujours fonctionner correctement
            Assert.Equal(ChangePhaseResult.SUCCESS, result);
            string finalInfo = room.GetPhaseInfo();
            Assert.Contains("Joueur 1", finalInfo);
            Assert.Contains("Placement", finalInfo);
        }

        /// <summary>
        /// TEST: Validation que les phases sont dans le bon ordre.
        /// </summary>
        [Fact]
        public async Task ChangePhase_PhaseSequence_FollowsCorrectOrder()
        {
            // ARRANGE
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();

            // ACT & ASSERT: Vérifier l'ordre exact des phases

            // Phase initiale: Draw (Player 1)
            Assert.Contains("Draw", room.GetPhaseInfo());
            Assert.Contains("Joueur 1", room.GetPhaseInfo());

            // Draw → Placement (Player 1)
            room.ChangePhase(player1Id, isManual: true);
            Assert.Contains("Placement", room.GetPhaseInfo());
            Assert.Contains("Joueur 1", room.GetPhaseInfo());

            // Placement → Attack (Player 1)
            room.ChangePhase(player1Id, isManual: true);
            Assert.Contains("Attack", room.GetPhaseInfo());
            Assert.Contains("Joueur 1", room.GetPhaseInfo());

            // Attack → Defense (switches to Player 2)
            room.ChangePhase(player1Id, isManual: true);
            Assert.Contains("Defense", room.GetPhaseInfo());
            Assert.Contains("Joueur 2", room.GetPhaseInfo()); // Player switched

            // Defense → EndTurn (Player 2)
            room.ChangePhase(player2Id, isManual: true);
            Assert.Contains("EndTurn", room.GetPhaseInfo());
            Assert.Contains("Joueur 2", room.GetPhaseInfo());

            // EndTurn → Draw (same player, new round)
            var result = room.ChangePhase(player2Id, isManual: true);
            Assert.Equal(ChangePhaseResult.PHASECOMPLETED, result);
            Assert.Contains("Draw", room.GetPhaseInfo());
            Assert.Contains("Joueur 2", room.GetPhaseInfo()); // Still Player 2
        }

        #endregion
    }
}
