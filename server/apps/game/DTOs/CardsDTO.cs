namespace Collection.DTOs
{
    public class CardDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Hp { get; set; }
        public int Attack { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; }= string.Empty;
        public string Picture { get; set; }= string.Empty;
        public int Effect_active { get; set; }
        public string CardType { get; set; }= string.Empty;
        public string Rarity { get; set; }= string.Empty;
        public string Extension { get; set; }= string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}