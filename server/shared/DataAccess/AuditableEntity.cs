namespace VortexTCG.DataAccess.Models
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedAtUtc { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime? UpdatedAtUtc { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
