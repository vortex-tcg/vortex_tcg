using System;
using System.ComponentModel.DataAnnotations;

namespace VortexTCG.Api.Deck.DTOs
{
    public class DeckDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public Guid ChampionId { get; set; }
        public Guid FactionId { get; set; }
    }

    public class DeckCreateDto
    {
        [Required, MinLength(1)]
        public string Label { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ChampionId { get; set; }

        [Required]
        public Guid FactionId { get; set; }
    }

    public class DeckUpdateDto
    {
        [Required, MinLength(1)]
        public string Label { get; set; } = string.Empty;

        [Required]
        public Guid ChampionId { get; set; }

        [Required]
        public Guid FactionId { get; set; }
    }
}
