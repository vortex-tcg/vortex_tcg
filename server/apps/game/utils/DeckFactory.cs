using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;
using VortexTCG.Game.Object;

namespace VortexTCG.Game.Utils
{
    public class DeckFactory
    {
        private static VortexTCG.Game.Object.Card genRandomCard(int i)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            CardDTO card = new CardDTO
            {
                Id = new Guid(),
                Name = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray()),
                Description = new string(Enumerable.Repeat(chars, 50).Select(s => s[random.Next(s.Length)]).ToArray()),
                Hp = random.Next(10),
                Attack = random.Next(10),
                Cost = random.Next(10),
                CardType = CardType.GUARD,
                Class = ["guerrier"]
            };
            return new VortexTCG.Game.Object.Card(card, i);
        }

        public static List<VortexTCG.Game.Object.Card> genDeck()
        {
            List<VortexTCG.Game.Object.Card> deck = new List<VortexTCG.Game.Object.Card>();

            for (int i = 0; i != 30; i +=1)
            {
                deck.Add(genRandomCard(i));
            }

            return deck;
        }
    }
}