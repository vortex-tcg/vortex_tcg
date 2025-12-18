using VortexTCG.Api.Collection.DTOs;
using VortexTCG.Api.Collection.Providers;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess.Models;
using CollectionModel = VortexTCG.DataAccess.Models.Collection;

namespace VortexTCG.Api.Collection.Services
{
    public class CollectionService
    {
        private readonly CollectionProvider _provider;
        public CollectionService(CollectionProvider provider)
        {
            _provider = provider;
        }

        private static CollectionDto Map(CollectionModel e) => new()
        {
            Id = e.Id
        };

        public async Task<ResultDTO<CollectionDto>> CreateAsync(CollectionCreateDto input, CancellationToken ct = default)
        {
            if (input.UserId == Guid.Empty)
                return new ResultDTO<CollectionDto> { success = false, statusCode = 400, message = "UserId requis" };

            CollectionModel entity = new CollectionModel
            {
                Id = Guid.NewGuid()
            };

            entity = await _provider.AddAsync(entity);
            return new ResultDTO<CollectionDto> { success = true, statusCode = 201, message = "Collection créée avec succès", data = Map(entity) };
        }

        public async Task<ResultDTO<CollectionDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            CollectionModel collection = await _provider.GetByIdAsync(id);
            if (collection == null)
                return new ResultDTO<CollectionDto> { success = false, statusCode = 404, message = "Collection non trouvée" };
            return new ResultDTO<CollectionDto> { success = true, statusCode = 200, data = Map(collection) };
        }

        public async Task<ResultDTO<CollectionDto[]>> GetAllAsync(CancellationToken ct = default)
        {
            List<CollectionModel> collections = await _provider.GetAllAsync();
            CollectionDto[] dtos = collections.ConvertAll(Map).ToArray();
            return new ResultDTO<CollectionDto[]> { success = true, statusCode = 200, data = dtos };
        }

        public async Task<ResultDTO<CollectionDto>> UpdateAsync(Guid id, CollectionCreateDto input, CancellationToken ct = default)
        {
            CollectionModel collection = await _provider.GetByIdAsync(id);
            if (collection == null)
                return new ResultDTO<CollectionDto> { success = false, statusCode = 404, message = "Collection non trouvée" };

            bool success = await _provider.UpdateAsync(collection);
            if (!success)
                return new ResultDTO<CollectionDto> { success = false, statusCode = 500, message = "Erreur lors de la mise à jour" };
            return new ResultDTO<CollectionDto> { success = true, statusCode = 200, message = "Collection mise à jour", data = Map(collection) };
        }

        public async Task<ResultDTO<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            bool success = await _provider.DeleteAsync(id);
            if (!success)
                return new ResultDTO<bool> { success = false, statusCode = 404, message = "Collection non trouvée" };
            return new ResultDTO<bool> { success = true, statusCode = 204, message = "Collection supprimée", data = true };
        }
    }
}
