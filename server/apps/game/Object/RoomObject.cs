namespace VortexTCG.Game.Object
{
    public class Room
    {
        private Guid _user_1;
        private Guid _user_2;

        private Deck _deck_user_1;
        private Deck _deck_user_2;


        private Hand _hand_user_1;
        private Hand _hand_user_2;

        private Champion _champion_user_1;
        private Champion _champion_user_2;

        private Graveyard _graveyard_user_1;
        private Graveyard _graveyard_user_2;

        private Board _board_user_1;
        private Board _board_user_2;

        public Room()
        {
            _deck_user_1 = new Deck();
            _deck_user_2 = new Deck();

            _hand_user_1 = new Hand();
            _hand_user_2 = new Hand();

            _champion_user_1 = new Champion();
            _champion_user_2 = new Champion();

            _graveyard_user_1 = new Graveyard();
            _graveyard_user_2 = new Graveyard();

            _board_user_1 = new Board();
            _board_user_2 = new Board();
        }

        public async Task setUser1(Guid user, Guid deck)
        {
            _user_1 = user;
            await _deck_user_1.initDeck(deck);
            await _champion_user_1.initChampion(deck);
        }

        public async Task setUser2(Guid user, Guid deck)
        {
            _user_2 = user;
            await _deck_user_2.initDeck(deck);
            await _champion_user_2.initChampion(deck);
        }

    }    
}