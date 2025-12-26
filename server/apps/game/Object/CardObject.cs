using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;

namespace VortexTCG.Game.Object
{
    public enum CardState {
        ENGAGE = 0,
        DEFENSE_ENGAGE = 1,
        ATTACK_ENGAGE = 2
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

        private readonly HashSet<CardState> _state;

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
            
            _state = new HashSet<CardState>();
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

        #region Gestion des états

            public void AddState(CardState newState) {
                if(!HasState(newState)) 
                    _state.Add(newState);
            }

            public bool HasState(CardState searchState) 
            => _state.Contains(searchState);

            public bool HasOneState(List<CardState> searchStates)
            => searchStates.Count > 0 && searchStates.Any(searchState => HasState(searchState));

            public bool HasStates(List<CardState> searchStates)
            => searchStates.Count > 0 && searchStates.All(searchState => HasState(searchState));

            public void RemoveState(CardState removeState)
            => _state.Remove(removeState);

            public void RemoveStates(HashSet<CardState> removeStates) {
                foreach(CardState state in removeStates) {
                    _state.Remove(state);
                }
            }

            public CardSlotState checkCanDefend() {
                if (HasState(CardState.DEFENSE_ENGAGE)) {
                    return CardSlotState.DEFENSE_ENGAGE;
                } else if (HasOneState([CardState.ATTACK_ENGAGE, CardState.ENGAGE])) {
                    return CardSlotState.ENGAGE;
                } else {
                    return CardSlotState.CAN_DEFEND;
                }
            }

            public CardSlotState checkCanAttack() {
                if (HasState(CardState.ATTACK_ENGAGE)) {
                    return CardSlotState.ATTACK_ENGAGE;
                } else if (HasOneState([CardState.DEFENSE_ENGAGE, CardState.ENGAGE])) {
                    return CardSlotState.ENGAGE;
                } else {
                    return CardSlotState.CAN_ATTACK;
                }
            }

        #endregion

        #region Format Dto

            public GameCardDto FormatGameCardDto() {
                return new GameCardDto {
                    Id = _card_id,
                    GameCardId = _game_card_id,
                    Name = _name,
                    Hp = _hp,
                    Attack = _attack,
                    Cost = _cost,
                    Description = _description,
                    CardType = _type,
                    Class = new List<string>(_class),
                    State = new List<CardState>(_state)
                };
            }

        #endregion
    }
}