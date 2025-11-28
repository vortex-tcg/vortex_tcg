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
            bool result1 = room.ChangePhase(player1Id, isManual: true);
            Assert.True(result1);
            string phaseInfo1 = room.GetPhaseInfo();
            Assert.Contains("Placement", phaseInfo1);
            Assert.Contains("Joueur 1", phaseInfo1);

            // ACT & ASSERT: Placement → Attack
            bool result2 = room.ChangePhase(player1Id, isManual: true);
            Assert.True(result2);
            string phaseInfo2 = room.GetPhaseInfo();
            Assert.Contains("Attack", phaseInfo2);

            // ACT & ASSERT: Attack → Defense
            bool result3 = room.ChangePhase(player1Id, isManual: true);
            Assert.True(result3);
            string phaseInfo3 = room.GetPhaseInfo();
            Assert.Contains("Defense", phaseInfo3);

            // ACT & ASSERT: Defense → EndTurn
            bool result4 = room.ChangePhase(player1Id, isManual: true);
            Assert.True(result4);
            string phaseInfo4 = room.GetPhaseInfo();
            Assert.Contains("EndTurn", phaseInfo4);

            // ACT & ASSERT: EndTurn → Draw (Player 2's turn)
            bool result5 = room.ChangePhase(player1Id, isManual: true);
            Assert.True(result5);
            string phaseInfo5 = room.GetPhaseInfo();
            Assert.Contains("Draw", phaseInfo5);
            Assert.Contains("Joueur 2", phaseInfo5);
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

            // ACT: Faire un tour complet pour Player 1
            room.ChangePhase(player1Id, isManual: true); // Draw → Placement
            room.ChangePhase(player1Id, isManual: true); // Placement → Attack
            room.ChangePhase(player1Id, isManual: true); // Attack → Defense
            room.ChangePhase(player1Id, isManual: true); // Defense → EndTurn
            room.ChangePhase(player1Id, isManual: true); // EndTurn → Draw (Player 2)

            // ASSERT: Maintenant c'est le tour du Player 2
            string infoPlayer2 = room.GetPhaseInfo();
            Assert.Contains("Joueur 2", infoPlayer2);
            Assert.Contains("Draw", infoPlayer2);

            // ACT: Faire un tour complet pour Player 2
            room.ChangePhase(player2Id, isManual: true); // Draw → Placement
            room.ChangePhase(player2Id, isManual: true); // Placement → Attack
            room.ChangePhase(player2Id, isManual: true); // Attack → Defense
            room.ChangePhase(player2Id, isManual: true); // Defense → EndTurn
            room.ChangePhase(player2Id, isManual: true); // EndTurn → Draw (Player 1)

            // ASSERT: De retour au Player 1
            string backToPlayer1 = room.GetPhaseInfo();
            Assert.Contains("Joueur 1", backToPlayer1);
            Assert.Contains("Draw", backToPlayer1);
        }

        #endregion

        #region Tests - Validation des Joueurs

        /// <summary>
        /// TEST: Seul le joueur actuel peut changer de phase.
        /// Les autres joueurs doivent être refusés.
        /// </summary>
        [Fact]
        public async Task ChangePhase_WrongPlayer_ReturnsFalse()
        {
            // ARRANGE: C'est le tour du Player 1 (début de partie)
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();

            // ACT: Player 2 essaie de changer la phase (pas son tour)
            bool result = room.ChangePhase(player2Id, isManual: true);

            // ASSERT: Doit être refusé
            Assert.False(result);

            // Vérifier que la phase n'a pas changé
            string phaseInfo = room.GetPhaseInfo();
            Assert.Contains("Draw", phaseInfo);
            Assert.Contains("Joueur 1", phaseInfo);
        }

        /// <summary>
        /// TEST: Validation avec un joueur inexistant.
        /// </summary>
        [Fact]
        public async Task ChangePhase_NonExistentPlayer_ReturnsFalse()
        {
            // ARRANGE
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();
            Guid fakePlayerId = Guid.NewGuid(); // Joueur qui n'existe pas dans la room

            // ACT
            bool result = room.ChangePhase(fakePlayerId, isManual: true);

            // ASSERT
            Assert.False(result);
        }

        #endregion

        #region Tests - Règles Spéciales Phase Placement

        /// <summary>
        /// TEST: La phase Placement ne peut être changée QUE manuellement.
        /// Les appels automatiques (serveur) doivent être refusés.
        /// </summary>
        [Fact]
        public async Task ChangePhase_PlacementPhaseAutomatic_ReturnsFalse()
        {
            // ARRANGE: Aller jusqu'à la phase Placement
            var (room, player1Id, player2Id) = await CreateConfiguredRoom();
            room.ChangePhase(player1Id, isManual: true); // Draw → Placement

            // Vérifier qu'on est bien en Placement
            Assert.Contains("Placement", room.GetPhaseInfo());

            // ACT: Essayer de changer automatiquement (serveur)
            bool automaticResult = room.ChangePhase(player1Id, isManual: false);

            // ASSERT: Doit être refusé
            Assert.False(automaticResult);

            // Vérifier qu'on est toujours en Placement
            Assert.Contains("Placement", room.GetPhaseInfo());

            // ACT: Changer manuellement (joueur)
            bool manualResult = room.ChangePhase(player1Id, isManual: true);

            // ASSERT: Doit fonctionner
            Assert.True(manualResult);
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

            // ACT & ASSERT: Attack phase - changement automatique
            bool attackResult = room.ChangePhase(player1Id, isManual: false);
            Assert.True(attackResult);
            Assert.Contains("Defense", room.GetPhaseInfo());

            // ACT & ASSERT: Defense phase - changement automatique  
            bool defenseResult = room.ChangePhase(player1Id, isManual: false);
            Assert.True(defenseResult);
            Assert.Contains("EndTurn", room.GetPhaseInfo());

            // ACT & ASSERT: EndTurn phase - changement automatique
            bool endTurnResult = room.ChangePhase(player1Id, isManual: false);
            Assert.True(endTurnResult);
            Assert.Contains("Joueur 2", room.GetPhaseInfo()); // Switch to player 2
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
            bool result1 = room.ChangePhase(player1Id, isManual: true); // Draw → Placement
            bool result2 = room.ChangePhase(player1Id, isManual: true); // Placement → Attack
            bool result3 = room.ChangePhase(player1Id, isManual: true); // Attack → Defense

            // ASSERT: Tous doivent fonctionner
            Assert.True(result1);
            Assert.True(result2);
            Assert.True(result3);

            // Vérifier l'état final
            Assert.Contains("Defense", room.GetPhaseInfo());
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

            // ACT: Premier tour complet (Player 1)
            room.ChangePhase(player1Id, isManual: true); // Draw → Placement
            room.ChangePhase(player1Id, isManual: true); // Placement → Attack
            room.ChangePhase(player1Id, isManual: true); // Attack → Defense
            room.ChangePhase(player1Id, isManual: true); // Defense → EndTurn
            room.ChangePhase(player1Id, isManual: true); // EndTurn → Draw (Player 2)

            // Deuxième tour complet (Player 2)
            room.ChangePhase(player2Id, isManual: true); // Draw → Placement
            room.ChangePhase(player2Id, isManual: true); // Placement → Attack
            room.ChangePhase(player2Id, isManual: true); // Attack → Defense
            room.ChangePhase(player2Id, isManual: true); // Defense → EndTurn
            room.ChangePhase(player2Id, isManual: true); // EndTurn → Draw (Player 1)

            // Troisième tour (Player 1 de nouveau)
            bool result = room.ChangePhase(player1Id, isManual: true); // Draw → Placement

            // ASSERT: Le système doit toujours fonctionner correctement
            Assert.True(result);
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

            // Phase initiale: Draw
            Assert.Contains("Draw", room.GetPhaseInfo());

            // Draw → Placement
            room.ChangePhase(player1Id, isManual: true);
            Assert.Contains("Placement", room.GetPhaseInfo());

            // Placement → Attack
            room.ChangePhase(player1Id, isManual: true);
            Assert.Contains("Attack", room.GetPhaseInfo());

            // Attack → Defense
            room.ChangePhase(player1Id, isManual: true);
            Assert.Contains("Defense", room.GetPhaseInfo());

            // Defense → EndTurn
            room.ChangePhase(player1Id, isManual: true);
            Assert.Contains("EndTurn", room.GetPhaseInfo());

            // EndTurn → Draw (next player)
            room.ChangePhase(player1Id, isManual: true);
            Assert.Contains("Draw", room.GetPhaseInfo());
            Assert.Contains("Joueur 2", room.GetPhaseInfo());
        }

        #endregion
    }
}
