namespace api.Effect.DTOs
{
    public class EffectDescriptionInputDto
    {
        public string Label { get; set; } = "";
        public string Description { get; set; } = "";
        public string? Parameter { get; set; } = null;
    }
    public class EffectDescriptionDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = "";
        public string Description { get; set; } = "";
        public string? Parameter { get; set; } = null;
    }
}