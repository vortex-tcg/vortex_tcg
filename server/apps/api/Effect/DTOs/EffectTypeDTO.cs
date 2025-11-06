using System.ComponentModel.DataAnnotations;

namespace api.Effect.DTOs
{
    public class EffectTypeDTO
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = default!;
    }

    public class EffectTypeCreateDTO
    {
        [Required, MinLength(1)]
        public string Label { get; set; } = default!;
    }

    public class EffectTypeUpdateDTO
    {
        [Required, MinLength(1)]
        public string Label { get; set; } = default!;
    }
}
