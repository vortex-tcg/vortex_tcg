using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Deck
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public int? UserId { get; set; }
        public User? Users { get; set; }
    }
}