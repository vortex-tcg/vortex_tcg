using VortexTCG.Api.Champion.DTOs;
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Interface;
using VortexTCG.Api.Deck.Providers;
using VortexTCG.Common.DTO;
using DeckModel = VortexTCG.DataAccess.Models.Deck;
using DeckCardModel = VortexTCG.DataAccess.Models.DeckCard;
using CollectionCardModel = VortexTCG.DataAccess.Models.CollectionCard;
using CardModel = VortexTCG.DataAccess.Models.Card;

namespace VortexTCG.Api.Deck.Services
{
    public class DeckService : IDeckService
    {
        private readonly DeckProvider _deckProvider;

        public DeckService(DeckProvider deckProvider)
        {
            _deckProvider = deckProvider;
        }
        public async Task<ResultDTO<bool>> UpdateDeckAsync(Guid deckId, UpdateDeckDto dto)
        {
            var deck = await _deckProvider.GetDeckForUpdateAsync(deckId);

            if (deck == null)
            {
                return new ResultDTO<bool>
                {
                    success = false,
                    statusCode = 404,
                    message = "Deck not found",
                    data = false
                };
            }

            deck.Label = dto.Name;
            deck.ChampionId = dto.ChampionId;
            deck.FactionId = dto.FactionId;

            await _deckProvider.UpdateDeckAsync(deck, dto.Cards);

            return new ResultDTO<bool>
            {
                success = true,
                statusCode = 200,
                data = true
            };
        }
        public async Task<ResultDTO<List<DeckDTO>>> GetDecksByUserIdAsync(Guid userId)
        {
            var decks = await _deckProvider.GetDecksByUserIdAsync(userId);

            var result = decks.Select(d => new DeckDTO
            {
                Id = d.Id.ToString(),
                Name = d.Label,
                Champion = d.Champion == null ? null : MapToDeckChampionDto(d.Champion)
            }).ToList();

            return new ResultDTO<List<DeckDTO>>
            {
                success = true,
                statusCode = 200,
                data = result
            };
        }

        public async Task<ResultDTO<DeckDataDto>> GetDeckDataAsync(Guid deckId)
        {
            DeckModel deck = await _deckProvider.GetDeckWithCardsAndChampionAsync(deckId);

            if (deck == null)
            {
                return new ResultDTO<DeckDataDto>
                {
                    success = false,
                    statusCode = 404,
                    message = "Deck not found",
                    data = null
                };
            }

            List<Guid> cardIds = deck.DeckCard
                .Select(dc => dc.Card.CardId)
                .Distinct()
                .ToList();

            List<(Guid CardId, string Label)> classRows = await _deckProvider.GetClassRowsByCardIdsAsync(cardIds);

            Dictionary<Guid, List<string>> classMap = classRows
                .Where(t => !string.IsNullOrWhiteSpace(t.Label))
                .GroupBy(t => t.CardId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => t.Label).Distinct().ToList()
                );

            List<DeckCardDto> cards = deck.DeckCard.Select((DeckCardModel dc) =>
            {
                CollectionCardModel cc = dc.Card;
                CardModel c = cc.Card;

                List<string> classes = classMap.TryGetValue(c.Id, out List<string>? labels)
                    ? labels
                    : new List<string>();

                return new DeckCardDto
                {
                    DeckCardId = dc.Id,
                    Quantity = dc.Quantity,
                    CollectionCardId = cc.Id,
                    Rarity = cc.Rarity,
                    CardId = c.Id,
                    Name = c.Name,
                    Hp = c.Hp,
                    Attack = c.Attack,
                    Cost = c.Cost,
                    Description = c.Description,
                    Picture = c.Picture,
                    Extension = c.Extension,
                    CardType = c.CardType,
                    Price = c.Price,
                    Classes = classes
                };
            }).ToList();

            DeckDataDto data = new DeckDataDto
            {
                Cards = cards,
                Champion = MapToDeckChampionDto(deck.Champion)
            };

            return new ResultDTO<DeckDataDto>
            {
                success = true,
                statusCode = 200,
                data = data
            };
        }

        private static DeckChampionDto MapToDeckChampionDto(DataAccess.Models.Champion ch)
        {
            return new DeckChampionDto
            {
                ChampionID = ch.Id,
                Name = ch.Name,
                Description = ch.Description,
                HP = ch.HP,
                Picture = ch.Picture,
                FactionId = ch.FactionId
            };
        }
    }
}