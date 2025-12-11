using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;
using VortexTCG.Game.Object;

namespace VortexTCG.Game.Utils
{
    public static class DeckFactory
    {
        private static VortexTCG.Game.Object.Card genRandomCard(int i)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            CardDTO card = new CardDTO
            {
                Id = Guid.NewGuid(),
                Name = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray()),
                Description = new string(Enumerable.Repeat(chars, 50).Select(s => s[random.Next(s.Length)]).ToArray()),
                Hp = random.Next(10),
                Attack = random.Next(10),
                Cost = random.Next(10),
                CardType = DataAccess.Models.CardType.GUARD,
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

        // --- MOCK POUR LE SERVEUR: génère des CardInstance aléatoires (Faction uniquement pour tests) ---
        public static List<VortexTCG.Game.Object.CardInstance> GenDeckInstances(int count = 30)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            List<VortexTCG.Game.Object.CardInstance> instances = new List<VortexTCG.Game.Object.CardInstance>(count);
            for (int i = 0; i < count; i++)
            {
                string name = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
                int atk = random.Next(1, 10);
                int hp = random.Next(1, 10);
                int cost = Math.Max(0, (atk + hp) / 3);

                instances.Add(new VortexTCG.Game.Object.CardInstance
                {
                    CardModelId = Guid.NewGuid(),
                    Name = name,
                    Type = VortexTCG.Game.Object.CardType.Faction,
                    Cost = cost,
                    Attack = atk,
                    Defense = hp,
                    Description = string.Empty,
                    Effects = new List<string>()
                });
            }
            return instances;
        }
    }
}