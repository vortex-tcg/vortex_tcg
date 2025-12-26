using VortexTCG.Game.DTO;
using VortexTCG.Game.Object;

namespace VortexTCG.Game.Object
{
    public class Champion
    {

        #region identifiant

            private Guid _deck_id;
            private Guid _id;

        #endregion

        #region hp
            
            private int _base_hp;
            private int _hp;
        
        #endregion

        #region gold

            private int _base_gold;
            private int _gold;

            private int _secondary_currency;

        #endregion

        #region fatigue
            
            private int _fatigue_counter;
        
        #endregion

        #region initialisation
            
            public void initChampion(Guid deck)
            {
                _id = Guid.NewGuid();
                _deck_id = deck;
                _base_hp = 30;
                _base_gold = 1;
                _gold = 1;
                _hp = 30;
                _fatigue_counter = 0;
            }

        #endregion

        #region getter setter
    
            public int GetHp() => _hp;
            public int GetBaseGold() => _base_gold;
            public int GetFatigue() => _fatigue_counter;

            public void SetBaseGold(int baseGold) => _base_gold = baseGold;

        #endregion

        #region Gestion des dégats
            public int ApplyDamage(Card card) {
                int appliedDamage = card.GetAttack();

                _hp -= appliedDamage;

                return appliedDamage;
            }

            public bool IsDead() => _hp <= 0;

            internal void ApplyFatigueDamage()
            {
                _fatigue_counter++;
                _hp -= _fatigue_counter;
            }

        #endregion

        #region Gestion de l'or

            public bool TryPaiedCard(int cost) => _gold >= cost;

            public void PayCard(int cost) => _gold -= cost;

            public void resetGold() => _gold = _base_gold;
    
        #endregion

        #region Format Dto

            public PlayCardChampionDto FormatPlayCardChampionDto()
            => new PlayCardChampionDto {
                Id = _id,
                Hp = _hp,
                Gold = _gold,
                SecondaryCurrency = _secondary_currency
            };

            public BattleChampionDto FormatBattleChampionDto()
            => new BattleChampionDto {
                Hp = _hp,
                SecondaryCurrency = _secondary_currency,
                Gold = _gold
            };
        
        #endregion

    }
}