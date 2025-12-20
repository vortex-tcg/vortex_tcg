using VortexTCG.DataAccess.Models;

namespace VortexTCG.Game.DTO
{
    /// <summary>
    /// Informations d'une carte piochée (envoyé au joueur).
    /// </summary>
    public class DrawnCardDTO
    {
        public int GameCardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Attack { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; } = string.Empty;
        public CardType CardType { get; set; }
    }

    /// <summary>
    /// Résultat de la pioche pour le joueur (avec les cartes).
    /// </summary>
    public class DrawResultForPlayerDTO
    {
        public Guid PlayerId { get; set; }
        public List<DrawnCardDTO> DrawnCards { get; set; } = new List<DrawnCardDTO>();
        public int FatigueCount { get; set; }
        public int BaseFatigue { get; set; }
    }

    /// <summary>
    /// Résultat de la pioche pour l'adversaire (sans les cartes).
    /// </summary>
    public class DrawResultForOpponentDTO
    {
        public Guid PlayerId { get; set; }
        public int CardsDrawnCount { get; set; }
        public int FatigueCount { get; set; }
        public int BaseFatigue { get; set; }
    }

    /// <summary>
    /// Résultat complet de la pioche.
    /// </summary>
    public class DrawCardsResultDTO
    {
        public DrawResultForPlayerDTO PlayerResult { get; set; } = new DrawResultForPlayerDTO();
        public DrawResultForOpponentDTO OpponentResult { get; set; } = new DrawResultForOpponentDTO();
    }
}
