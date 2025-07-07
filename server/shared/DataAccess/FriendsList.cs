using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class FriendsList
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}