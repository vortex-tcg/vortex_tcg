using System.ComponentModel.DataAnnotations;

namespace VortexTCG.Api.Rank.DTOs
{
    // DTO retourné par l'API
    public class RankDTO
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = "";
        public int nbVictory { get; set; } = 0;
    }

    // DTO utilisé pour la création
    public class RankCreateDTO
    {
        [Required, MinLength(1)]
        public string Label { get; set; } = default!;
        [Range(0, int.MaxValue)]
        public int nbVictory { get; set; }
    }
}
