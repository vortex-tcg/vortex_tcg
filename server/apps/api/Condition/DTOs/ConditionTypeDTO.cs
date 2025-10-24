using VortexTCG.DataAccess.Models;

namespace VortexTCG.API.DTO
{
    public class ConditionTypeCreateDTO
    {
        public string label { get; set; } = "";
        public string[] conditions { get; set; } = [];
    }

    public class ConditionTypeCreateResponseDTO
    {
        public Guid id { get; set; } = default!;
        public string? label { get; set; } = null;
        public ICollection<Condition> conditions { get; set; } = [];
    }
}