using System.ComponentModel.DataAnnotations;

namespace VortexTCG.Api.Card.DTOs
{
    // DTO de ce qui est retournée
    public class CardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public int Price { get; set; } = 0;
        public int Hp { get; set; } = 0;
        public int Attack  { get; set; } = 0;
        public int Cost { get; set; } = 0;
        public string Description { get; set; } = default!;
        public string Picture { get; set; } = default!;
        public string Extension { get; set; } = default!;
        public string CardType { get; set; } = default!;
        public List<string> Class { get; set; } = new List<string>();
        public List<Guid> Factions { get; set; } = new List<Guid>();
    }

    public class CardCreateDto
    {
        [Required, MinLength(1)]
        public string Name { get; set; } = default!;
        
        [Range(0, int.MaxValue)]
        public int Price { get; set; }
        
        [Range(0, int.MaxValue)]
        public int Hp { get; set; }
        
        [Range(0, int.MaxValue)]
        public int Attack { get; set; }
        
        [Required, MinLength(1)]
        public string Description { get; set; } = default!;
        [Required, MinLength(1)]
        public string Picture { get; set; } = default!;
    }
}