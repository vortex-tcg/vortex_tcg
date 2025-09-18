namespace Collection.DTOs
{
    public class CardDTO
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public int hp { get; set; }
        public int attack { get; set; }
        public int cost { get; set; }
        public string description { get; set; }= string.Empty;
        public string picture { get; set; }= string.Empty;
        public int effect_active { get; set; }
        public int card_type_id { get; set; }
        public int rarity_id { get; set; }
        public int extension_id { get; set; }
        public DateTime created_at { get; set; }
    }
}