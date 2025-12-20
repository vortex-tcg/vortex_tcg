namespace VortexTCG.Game.Object
{
    public class Champion
    {
        private Guid _deck_id;

        private int _base_hp;
        private int _hp;

        private int _base_gold;
        private int _gold;

        private int _secondary_currency;
        private int _fatigue_counter;

        public void initChampion(Guid deck)
        {
            _deck_id = deck;
            _base_hp = 30;
            _base_gold = 1;
            _gold = 1;
            _hp = 30;
            _fatigue_counter = 0;
        }

        public int GetHp() => _hp;
        public int GetBaseGold() => _base_gold;
        public int GetFatigue() => _fatigue_counter;

        public void SetBaseGold(int baseGold) {
            _base_gold = baseGold;
        }

        internal void ApplyFatigueDamage()
        {
            _fatigue_counter++;
            _hp -= _fatigue_counter;
        }

        public bool TryPaiedCard(int cost) {
            if (_gold < cost) {
                return false;
            }
            return true;
        }

        public void PayCard(int cost) {
            _gold -= cost;
        }

        public void resetGold() {
            _gold = _base_gold;
        }

    }
}