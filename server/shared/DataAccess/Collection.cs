using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Collection
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public ICollection<User> Users { get; set; }
    }
}