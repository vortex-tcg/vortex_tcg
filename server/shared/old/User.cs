using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public int Rank { get; set; }

        public int CurrencyQuantity { get; set; }

        public string Language { get; set; }

        // Relations

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public ICollection<Booster> Boosters { get; set; }

        public ICollection<Game> GamesAsPlayer { get; set; }

        public ICollection<Game> GamesAsCurrentPlayer { get; set; }

        public ICollection<FriendsList> FriendsLists { get; set; }
    }
}
