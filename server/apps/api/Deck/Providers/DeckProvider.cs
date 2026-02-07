using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Champion.DTOs;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using DeckModel = VortexTCG.DataAccess.Models.Deck;
using CardModel = VortexTCG.DataAccess.Models.Card;
using DeckCardModel = VortexTCG.DataAccess.Models.DeckCard;
using CollectionCardModel = VortexTCG.DataAccess.Models.CollectionCard;
using ClassCardModel = VortexTCG.DataAccess.Models.ClassCard;


namespace VortexTCG.Api.Deck.Providers
{
    public class DeckProvider
    {
        private readonly VortexDbContext _db;

        public DeckProvider(VortexDbContext db)
        {
            _db = db;
        }
        public DeckDTO GetMockDeck(string deckId)
        {
            List<VortexTCG.Api.Card.DTOs.CardDto> cards = new List<VortexTCG.Api.Card.DTOs.CardDto>();
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            DeckChampionDto champion = new DeckChampionDto {
                ChampionID = new Guid(),
                Name = "Emporio PingChilling",
                Description = "Le premier empereur berzerkouin",
                HP = 30,
            };

            for (int i = 0; i < 30; i++)
            {
                VortexTCG.Api.Card.DTOs.CardDto card = new VortexTCG.Api.Card.DTOs.CardDto
                {
                    Id = Guid.NewGuid(),
                    Name = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray()),
                    Description = new string(Enumerable.Repeat(chars, 50).Select(s => s[random.Next(s.Length)]).ToArray()),
                    Hp = random.Next(1, 10),
                    Attack = random.Next(1, 10),
                    Cost = random.Next(1, 10),
                    Price = random.Next(1, 10),
                    Picture = "mock.png",
                    Extension = "BASIC",
                    CardType = "GUARD",
                    Class = new List<string> { "guerrier" },
                    Factions = new List<Guid>()
                };
                cards.Add(card);
            }
            DeckDTO deck = new DeckDTO
            {
                Id = deckId,
                Name = $"Mock Deck {deckId}",
                Cards = cards,
                Champion = champion
            };
            return deck;
        }

        public async Task<DeckDataDto?> GetDeckDataAsync(Guid deckId)
        {
            DeckModel? deck = await _db.Decks
                .AsNoTracking()
                .Include(d => d.DeckCard)
                .ThenInclude((DeckCardModel dc) => dc.Card) 
                .ThenInclude((CollectionCardModel cc) => cc.Card) 
                .Include(d => d.Champion)
                .FirstOrDefaultAsync(d => d.Id == deckId);

            if (deck == null) return null;

            List<Guid> cardIds = deck.DeckCard
                .Select((DeckCardModel dc) => dc.Card.CardId) 
                .Distinct()
                .ToList();

            List<(Guid CardId, string Label)> classRows = await _db.Set<ClassCardModel>()
                .AsNoTracking()
                .Include((ClassCardModel x) => x.Class)
                .Where((ClassCardModel x) => cardIds.Contains(x.CardId))
                .Select((ClassCardModel x) => new ValueTuple<Guid, string>(
                    x.CardId,
                    x.Class != null ? x.Class.Label : ""
                ))
                .ToListAsync();

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

            return new DeckDataDto
            {
                Cards = cards,
                Champion = MapToDeckChampionDto(deck.Champion)
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
