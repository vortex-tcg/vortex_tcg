using VortexTCG.DataAccess.Models;

namespace VortexTCG.Game.DTO
{
    public class CardDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;

        public int Hp { get; set; } = default!;

        public int Attack { get; set; } = default!;

        public int Cost { get; set; } = default!;

        public string Description { get; set; } = default!;

        public CardType CardType { get; set; } = default!;

        public ICollection<string> Class { get; set; } = default!;

    }

    public class GameCardDto
    {
        public Guid Id { get; set; }

        public int GameCardId { get; set; } = default!;

        public string Name { get; set; } = default!;

        public int Hp { get; set; } = default!;

        public int Attack { get; set; } = default!;

        public int Cost { get; set; } = default!;

        public string Description { get; set; } = default!;

        public CardType CardType { get; set; } = default!;

        public ICollection<string> Class { get; set; } = default!;   
    }
}