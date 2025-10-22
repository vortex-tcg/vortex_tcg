using System.Collections.Generic; //Permet l’utilisation des listes (ICollection<T>) pour les relations 1-N ou N-N.
using System.ComponentModel.DataAnnotations; //Permet d’utiliser les attributs [Key], [Required], [EmailAddress], etc.
using System.ComponentModel.DataAnnotations.Schema; //Utilisé pour des attributs comme [ForeignKey] ou [Table] si nécessaire.

namespace VortexTCG.DataAccess.Models
{
    public enum Role { USER = 0, ADMIN = 1, SUPER_ADMIN = 2 };
    public enum UserStatus { DISCONNECTED = 0, CONNECTED = 1, IN_QUEUE = 2, IN_GAME = 3 };

    public class User : AuditableEntity //Cette classe deviendra une table Users (par convention, EF Core met le nom de la classe au pluriel pour la table).
    {
        [Key] //Primary Key
        public Guid Id { get; set; }

        [Required]
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

        [Required]
        public string Username { get; set; } = default!;

        [Required] //Met en Not Null, uniquement pour autre chose qu'un INT car ils sont automatiquement non nullable
        public string Password { get; set; } = default!;

        [Required]
        [EmailAddress] //Ajoute une validation au runtime, mais n'a pas d'effet en base.
        public string Email { get; set; } = default!;

        public int CurrencyQuantity { get; set; }

        public string Language { get; set; } = default!;

        // Foreign Keys

        public Role Role { get; set; } = default!;
        public UserStatus Status { get; set; } = default!;

        public Guid? RankId { get; set; } = default!;
        public Rank? Rank { get; set; } = default!;

        public Guid? CollectionId { get; set; }
        public Collection? Collection { get; set; }

        public ICollection<Deck>? Decks { get; set; }

        public ICollection<Game>? Logs { get; set; }

        public ICollection<Friend> Friends { get; set; } // Quand j'ajoute un ami
        public ICollection<Friend> OtherFriends { get; set; } // Quand des utilisateurs m'ajoute en ami
    }

    public class Friend : AuditableEntity
    {
        [Key] //Primary Key
        public Guid Id { get; set; }

        public Guid FriendUserId { get; set; }
        public User FriendUser { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
