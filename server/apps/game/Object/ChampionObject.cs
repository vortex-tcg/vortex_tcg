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


        public void initChampion(Guid deck)
        {
            _deck_id = deck;
            _base_hp = 30;
            _base_gold = 1;
            _gold = 1;
            _hp = 30;
            return;
        }
    }
}