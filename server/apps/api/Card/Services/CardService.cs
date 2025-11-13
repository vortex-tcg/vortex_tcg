using api.Card.DTOs;
using api.Card.Providers;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess.Models;
using CardModel = VortexTCG.DataAccess.Models.Card;

namespace api.Card.Services
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

        public async Task<ResultDTO<List<CardDTO>>> listAsync(CancellationToken ct = default)
        {
            var entities = await _provider.listAsync(ct);
            return new ResultDTO<List<CardDTO>>
            {
                success = true,
                statusCode = 200,
                data = entities.Select(Map).ToList()
            };
        }

        public async Task<ResultDTO<CardDTO>> createAsync(CardCreateDTO input, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                return new ResultDTO<CardDTO> { success = false, statusCode = 400, message = "Name requis" };

            if (await _provider.existsByNameAsync(input.Name, ct))
                return new ResultDTO<CardDTO> { success = false, statusCode = 409, message = "Une carte avec ce nom existe déjà" };

            var entity = new CardModel
            {
                Id = Guid.NewGuid(),
                Name = input.Name,
                Description = input.Description,
                Attack = input.Attack,
                Hp = input.Hp,
                Price = input.Price,
                Picture = input.Picture
            };
            
            entity = await _provider.addAsync(entity, ct);
            return new ResultDTO<CardDTO> { success = true, statusCode = 201, message = "Carte crée avec succès" };
        }
    }
}