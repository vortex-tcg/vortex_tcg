using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Class
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public int? CardId { get; set; }
        public Card? Cards { get; set; }

    }
}