namespace VortexTCG.Cards.DTOs
{
    /// <summary>
    /// Objet de transfert de données pour représenter une carte.
    /// </summary>
    public class CardDTO
    {
        /// <summary>
        /// Identifiant unique de la carte.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Nom de la carte.
        /// </summary>
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// Points de vie de la carte.
        /// </summary>
        public int hp { get; set; }

        /// <summary>
        /// Points d'attaque de la carte.
        /// </summary>
        public int attack { get; set; }

        /// <summary>
        /// Coût de la carte.
        /// </summary>
        public int cost { get; set; }

        /// <summary>
        /// Description de la carte.
        /// </summary>
        public string description { get; set; } = string.Empty;

        /// <summary>
        /// URL de l'image associée à la carte.
        /// </summary>
        public string picture { get; set; } = string.Empty;

        /// <summary>
        /// Indique si un effet est actif sur la carte.
        /// </summary>
        public int effect_active { get; set; }

        /// <summary>
        /// Identifiant du type de carte.
        /// </summary>
        public int card_type_id { get; set; }

        /// <summary>
        /// Identifiant de la rareté de la carte.
        /// </summary>
        public int rarity_id { get; set; }

        /// <summary>
        /// Identifiant de l'extension de la carte.
        /// </summary>
        public int extension_id { get; set; }

        /// <summary>
        /// Date de création de la carte.
        /// </summary>
        public DateTime created_at { get; set; }
    }
}