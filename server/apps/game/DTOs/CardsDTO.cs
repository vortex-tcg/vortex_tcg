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
        public int CardTypeId { get; set; }
        public int RarityId { get; set; }
        public int ExtensionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}