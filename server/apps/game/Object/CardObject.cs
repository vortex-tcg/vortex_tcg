using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;

namespace VortexTCG.Game.Object
{
    public class Card
    {

        private readonly Guid _card_id;
        private readonly int _game_card_id;

        private readonly string _name;

        private readonly int _base_hp;
        private int _hp;

        private readonly int _base_attack;
        private int _attack;

        private readonly int _base_cost;
        private int _cost;

        private readonly string _description;

        private readonly CardType _type;

        private readonly List<Effect> _effects;

        private readonly List<string> _class;

        public Card(CardDTO card, int id)
        {
            _card_id = card.Id;
            _game_card_id = id;

            _name = card.Name;
            _description = card.Description;

            _base_hp = card.Hp;
            _base_attack = card.Attack;
            _base_cost = card.Cost;
        
            _hp = card.Hp;
            _attack = card.Attack;
            _cost = card.Cost;

            _type = card.CardType;

            _class = new List<string>(card.Class);

            _effects = new List<Effect>();
        }
    }
}