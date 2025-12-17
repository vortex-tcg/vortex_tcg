using System.ComponentModel.DataAnnotations;

namespace VortexTCG.Api.Collection.DTOs
{
    // DTO retourné par l'API
    public class CollectionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
    }

    // DTO utilisé pour la création
    public class CollectionCreateDto
    {
        public required Guid UserId { get; set; }
    }
}
