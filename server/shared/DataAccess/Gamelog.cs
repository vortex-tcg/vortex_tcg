using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{

    public enum GameEndStatus { WIN = 0, LOOSE = 1, SURRENDER = 2, DISCONNECT = 3 };

    public class Gamelog : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Label { get; set; } = default!;

        public int TurnNumber { get; set; } = default!;

        public Game? User { get; set; } = default!;

        public ICollection<ActionType>? Actions { get; set; } = default!;
    }

    public class ActionType : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string actionDescription { get; set; } = default!;

        public Guid GameLogId { get; set; } = default!;
        public Gamelog Gamelog { get; set; } = default!;

        public Guid ParentId { get; set; } = default!;
        public ActionType Parent { get; set; } = default!;

        public ICollection<ActionType> Childs { get; set; } = default!;
    }

    public class Game : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public GameEndStatus Status { get; set; } = default!;

        public Guid UserId { get; set; } = default!;
        public User User { get; set; } = default!;

        public Guid GamelogId { get; set; } = default!;
        public Gamelog Gamelog { get; set; } = default!;
    }
}