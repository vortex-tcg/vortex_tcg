namespace VortexTCG.Class.DTOs
{
    public class ClassCreateDTO
    {
        public string Label { get; set; } = default!;
    }

    public class ClassDTO
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = default!;
        public ICollection<CardDTO> Cards { get; set; } = default!;
    }
   
}
