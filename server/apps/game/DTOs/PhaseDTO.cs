namespace VortexTCG.Game.DTO
{
    /// <summary>
    /// Les différentes phases d'un tour de jeu.
    /// </summary>
    public enum GamePhase
    {
        /// <summary>Phase de pioche - Le joueur pioche une carte</summary>
        Draw = 0,

        /// <summary>Phase de placement - Le joueur peut poser des cartes sur le board</summary>
        Placement = 1,

        /// <summary>Phase d'attaque - Le joueur peut attaquer avec ses créatures</summary>
        Attack = 2,

        /// <summary>Phase de défense - L'adversaire peut défendre</summary>
        Defense = 3,

        /// <summary>Phase de fin de tour - Résolution des effets de fin de tour</summary>
        EndTurn = 4
    }

    /// <summary>
    /// Résultat d'un changement de phase envoyé au joueur actif.
    /// </summary>
    public class PhaseChangeResultDTO
    {
        /// <summary>Nouvelle phase actuelle</summary>
        public GamePhase CurrentPhase { get; set; }

        /// <summary>ID du joueur dont c'est le tour</summary>
        public Guid ActivePlayerId { get; set; }

        /// <summary>Numéro du tour actuel</summary>
        public int TurnNumber { get; set; }

        /// <summary>Indique si la phase a été changée automatiquement par le serveur</summary>
        public bool AutoChanged { get; set; }

        /// <summary>Raison du changement automatique (si applicable)</summary>
        public string? AutoChangeReason { get; set; }

        /// <summary>Indique si le joueur peut effectuer des actions dans cette phase</summary>
        public bool CanAct { get; set; }
    }

    /// <summary>
    /// Résultat d'un changement de phase envoyé à l'adversaire.
    /// </summary>
    public class PhaseChangeForOpponentDTO
    {
        /// <summary>Nouvelle phase actuelle</summary>
        public GamePhase CurrentPhase { get; set; }

        /// <summary>ID du joueur dont c'est le tour</summary>
        public Guid ActivePlayerId { get; set; }

        /// <summary>Numéro du tour actuel</summary>
        public int TurnNumber { get; set; }

        /// <summary>Indique si c'est maintenant à l'adversaire de jouer (phase de défense)</summary>
        public bool IsYourTurnToAct { get; set; }
    }

    /// <summary>
    /// Résultat complet du changement de phase.
    /// </summary>
    public class ChangePhaseResultDTO
    {
        /// <summary>Résultat pour le joueur actif</summary>
        public PhaseChangeResultDTO ActivePlayerResult { get; set; } = new();

        /// <summary>Résultat pour l'adversaire</summary>
        public PhaseChangeForOpponentDTO OpponentResult { get; set; } = new();

        /// <summary>Indique si le tour a changé de joueur</summary>
        public bool TurnChanged { get; set; }
    }
}
