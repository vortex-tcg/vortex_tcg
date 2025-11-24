using System.ComponentModel.DataAnnotations;

namespace VortexTCG.Api.Collection.DTOs
{
    // DTO retourné par l'API
    public class CollectionDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }

    // DTO utilisé pour la création
    public class CollectionCreateDTO
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
