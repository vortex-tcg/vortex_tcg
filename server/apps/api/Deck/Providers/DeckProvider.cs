using Deck.DTOs;

namespace Deck.Providers
{
    public class DeckProvider
    {
        public DeckDTO GetMockDeck(string deckId)
        {
            List<VortexTCG.Api.Card.DTOs.CardDto> cards = new List<VortexTCG.Api.Card.DTOs.CardDto>();
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
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
                Cards = cards
            };
            return deck;
        }
    }
}
