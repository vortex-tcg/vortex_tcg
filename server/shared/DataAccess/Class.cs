using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Class : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Label { get; set; } = default!;

        public ICollection<ClassCard> Cards { get; set; } = default!;

    }
    
    public class ClassCard : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CardId { get; set; } = Guid.Empty;
        public Card Card { get; set; } = default!;

        public Guid ClassId { get; set; } = Guid.Empty;
        public Class Class { get; set; } = default!;
    }

}