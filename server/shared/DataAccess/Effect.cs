using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Effect : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Title { get; set; } = default!;

        public string Parameter { get; set; } = default!;

        public Guid EffectTypeId { get; set; } = Guid.Empty;
        public EffectType EffectType { get; set; } = default!;

        public Guid EffectDescriptionId { get; set; } = Guid.Empty;
        public EffectDescription EffectDescription { get; set; } = default!;

        public Guid StartConditionId { get; set; } = Guid.Empty;
        public Condition StartCondition { get; set; } = default!;

        public Guid EndConditionId { get; set; } = Guid.Empty;
        public Condition EndCondition { get; set; } = default!;
        
        public Champion Champion { get; set; } = default!;

        public ICollection<EffectCard> Cards { get; set; } = default!;
    }

    public class EffectCard : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid EffectId { get; set; } = Guid.Empty;
        public Effect Effect { get; set; } = default!;

        public Guid CardId { get; set; } = Guid.Empty;
        public Card Card { get; set; } = default!;

    }

    public class EffectDescription : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Label { get; set; } = default!;

        public string Description { get; set; } = default!;

        public string? Parameter { get; set; } = default!;

        public ICollection<Effect> Effects { get; set; } = default!;
    }

    public class EffectType : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Label { get; set; } = default!;
        
        public ICollection<Effect> Effect { get; set; } = default!;
    }
}