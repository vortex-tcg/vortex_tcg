using VortexTCG.Api.Card.DTOs;
using VortexTCG.Api.Card.Providers;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess.Models;
using CardModel = VortexTCG.DataAccess.Models.Card;

namespace VortexTCG.Api.Card.Services
{
    public class CardService
    {
        private readonly CardProvider _provider;
        
        private static CardDTO Map(CardModel e) => new()
        {
            Id = e.Id,
            Name = e.Name
        };

        public CardService(CardProvider provider)
        {
            _provider = provider;
        }

        public async Task<ResultDTO<CardDTO[]>> GetAllAsync(CancellationToken ct = default)
        {
            List<CardModel> entities = await _provider.GetAllAsync(ct);
            ResultDTO<CardDTO[]> result = new ResultDTO<CardDTO[]>
            {
                success = true,
                statusCode = 200,
                data = entities.Select(Map).ToArray()
            };
            return result;
        }

        public async Task<ResultDTO<CardDTO>> CreateAsync(CardCreateDTO input, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                return new ResultDTO<CardDTO> { success = false, statusCode = 400, message = "Name requis" };

            if (await _provider.ExistsByNameAsync(input.Name, ct))
                return new ResultDTO<CardDTO> { success = false, statusCode = 409, message = "Une carte avec ce nom existe déjà" };

            CardModel entity = new CardModel
            {
                Id = Guid.NewGuid(),
                Name = input.Name,
                Description = input.Description,
                Attack = input.Attack,
                Hp = input.Hp,
                Price = input.Price,
                Picture = input.Picture
            };
            
            entity = await _provider.AddAsync(entity, ct);
            ResultDTO<CardDTO> created = new ResultDTO<CardDTO>
            {
                success = true,
                statusCode = 201,
                message = "Carte crée avec succès",
                data = Map(entity)
            };
            return created;
        }

        public async Task<ResultDTO<CardDTO>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            CardModel? entity = await _provider.GetByIdAsync(id, ct);
            if (entity is null)
            {
                ResultDTO<CardDTO> notFound = new ResultDTO<CardDTO>
                {
                    success = false,
                    statusCode = 404,
                    message = "Carte non trouvée"
                };
                return notFound;
            }
            ResultDTO<CardDTO> ok = new ResultDTO<CardDTO>
            {
                success = true,
                statusCode = 200,
                data = Map(entity)
            };
            return ok;
        }
    }
}