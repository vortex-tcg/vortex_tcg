using System.ComponentModel.DataAnnotations;

namespace api.Effect.DTOs
{
    public class EffectTypeDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = default!;
    }

    public class EffectTypeCreateDto
    {
        [Required, MinLength(1)]
        public string Label { get; set; } = default!;
    }

    public class EffectTypeUpdateDto
    {
        [Required, MinLength(1)]
        public string Label { get; set; } = default!;
    }
}
