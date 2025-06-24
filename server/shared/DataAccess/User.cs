using System.Collections.Generic; //Permet l’utilisation des listes (ICollection<T>) pour les relations 1-N ou N-N.
using System.ComponentModel.DataAnnotations; //Permet d’utiliser les attributs [Key], [Required], [EmailAddress], etc.
using System.ComponentModel.DataAnnotations.Schema; //Utilisé pour des attributs comme [ForeignKey] ou [Table] si nécessaire.



namespace VortexTCG.Models
{
    public class User //Cette classe deviendra une table Users (par convention, EF Core met le nom de la classe au pluriel pour la table).
    {
        [Key] //Primary Key
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Username { get; set; }

        [Required] //Met en Not Null, uniquement pour autre chose qu'un INT car ils sont automatiquement non nullable
        public string Password { get; set; }

        [Required]
        [EmailAddress] //Ajoute une validation au runtime, mais n'a pas d'effet en base.
        public string Email { get; set; }

        public int CurrencyQuantity { get; set; }

        public string Language { get; set; }

        // Foreign Keys

        public int RoleId { get; set; }
        public Role Role { get; set; }
        public int RankId { get; set; }
        public Rank Rank { get; set; }

        public ICollection<Booster> Boosters { get; set; }

        public ICollection<Game> GamesAsPlayer { get; set; }

        public ICollection<Game> GamesAsCurrentPlayer { get; set; }

        public ICollection<FriendsList> FriendsLists { get; set; }
    }
}
