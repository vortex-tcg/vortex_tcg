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

        private static CollectionDTO Map(CollectionModel e) => new()
        {
            Id = e.Id
        };

        public async Task<ResultDTO<CollectionDTO>> CreateAsync(CollectionCreateDTO input, CancellationToken ct = default)
        {
            if (input.UserId == Guid.Empty)
                return new ResultDTO<CollectionDTO> { success = false, statusCode = 400, message = "UserId requis" };

            var entity = new CollectionModel
            {
                Id = Guid.NewGuid()
            };

            entity = await _provider.AddAsync(entity);
            return new ResultDTO<CollectionDTO> { success = true, statusCode = 201, message = "Collection créée avec succès", data = Map(entity) };
        }

        public async Task<ResultDTO<CollectionDTO>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var collection = await _provider.GetByIdAsync(id);
            if (collection == null)
                return new ResultDTO<CollectionDTO> { success = false, statusCode = 404, message = "Collection non trouvée" };
            return new ResultDTO<CollectionDTO> { success = true, statusCode = 200, data = Map(collection) };
        }

        public async Task<ResultDTO<CollectionDTO[]>> GetAllAsync(CancellationToken ct = default)
        {
            var collections = await _provider.GetAllAsync();
            var dtos = collections.ConvertAll(Map).ToArray();
            return new ResultDTO<CollectionDTO[]> { success = true, statusCode = 200, data = dtos };
        }

        public async Task<ResultDTO<CollectionDTO>> UpdateAsync(Guid id, CollectionCreateDTO input, CancellationToken ct = default)
        {
            var collection = await _provider.GetByIdAsync(id);
            if (collection == null)
                return new ResultDTO<CollectionDTO> { success = false, statusCode = 404, message = "Collection non trouvée" };

            var success = await _provider.UpdateAsync(collection);
            if (!success)
                return new ResultDTO<CollectionDTO> { success = false, statusCode = 500, message = "Erreur lors de la mise à jour" };
            return new ResultDTO<CollectionDTO> { success = true, statusCode = 200, message = "Collection mise à jour", data = Map(collection) };
        }

        public async Task<ResultDTO<object>> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var success = await _provider.DeleteAsync(id);
            if (!success)
                return new ResultDTO<object> { success = false, statusCode = 404, message = "Collection non trouvée" };
            return new ResultDTO<object> { success = true, statusCode = 204, message = "Collection supprimée" };
        }
    }
}
