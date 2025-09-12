namespace Collection.DTOs
{
    /// <summary>
    /// Objet de transfert de données pour représenter une carte.
    /// </summary>
    public class CardDTO
    {
        /// <summary>
        /// Identifiant unique de la carte.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nom de la carte.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Points de vie de la carte.
        /// </summary>
        public int Hp { get; set; }

        /// <summary>
        /// Points d'attaque de la carte.
        /// </summary>
        public int Attack { get; set; }

        /// <summary>
        /// Coût de la carte.
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// Description de la carte.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// URL de l'image associée à la carte.
        /// </summary>
        public string Picture { get; set; } = string.Empty;

        /// <summary>
        /// Indique si un effet est actif sur la carte.
        /// </summary>
        public int Effect_active { get; set; }

        /// <summary>
        /// Identifiant du type de carte.
        /// </summary>
        public int CardTypeId { get; set; }

        /// <summary>
        /// Identifiant de la rareté de la carte.
        /// </summary>
        public int RarityId { get; set; }

        /// <summary>
        /// Identifiant de l'extension de la carte.
        /// </summary>
        public int ExtensionId { get; set; }

        /// <summary>
        /// Date de création de la carte.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}