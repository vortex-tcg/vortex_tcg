using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;

namespace VortexTCG.Game.Object
{
    public enum CardState {
        ENGAGE = 0
        BATTLE_ENGAGE = 1,
        DEFENSE_ENGAGE = 2
    }

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

        private List<CardState> _state;

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

        public int GetGameCardId() => _game_card_id;
        public Guid GetCardId() => _card_id;
        public string GetName() => _name;
        public int GetHp() => _hp;
        public int GetAttack() => _attack;
        public int GetCost() => _cost;
        public string GetDescription() => _description;
        public CardType GetCardType() => _type;
        public List<string> GetClasses() => new List<string>(_class);
        public List<CardState> GetState() => new List<CardState>(_state);

        public void AddState(CardState newState) {
            bool isNewUniqueState = true;
            foreach(CardState state in _state) {
                if (state == newState) {
                    isNewUniqueState = false;
                }
            }
            if (isNewUniqueState) {
                _state.Add(newState);
            }
        }

        public bool HasState(CardState searchState) {
            foreach(CardState state in _state) {
                if (searchState == state) {
                    return true;
                }
            }
            return false;
        }

        public void RemoveState(CardState removeState)
        => _state.Remove(removeState);
    }
}