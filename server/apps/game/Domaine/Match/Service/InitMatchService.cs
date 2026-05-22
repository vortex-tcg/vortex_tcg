    using game.Domaine.Match.Agregate;
    using game.Domaine.Match.Entity;
    using game.Domaine.Match.ValueObject;

    namespace game.Domaine.Match.Service;

    public static class InitMatchService
    {
        public static MatchInitData Init(Agregate.Match match)
        {
            Random rng = new Random();

            match.Player1.Deck.Shuffle(rng);
            match.Player2.Deck.Shuffle(rng);
            match.Player1.Champion.Gold = new ChampionGold(match.Player1.Champion.BaseGold.Value);
            match.Player2.Champion.Gold = new ChampionGold(match.Player2.Champion.BaseGold.Value);
            match.Player1.Champion.Hp = new ChampionHp(3);
            match.Player2.Champion.Hp = new ChampionHp(3);
            match.Player1.Champion.BaseHp = new ChampionBaseHp(3);
            match.Player2.Champion.BaseHp = new ChampionBaseHp(3);

            int p1Position = 1;
            int p2Position = 2;

            List<GameCardDto> p1Drawn = DrawCardsService.DrawCards(match.Player1, 6);
            List<GameCardDto> p2Drawn = DrawCardsService.DrawCards(match.Player2, 5);

            MatchInitData init = new MatchInitData(
                match.MatchId.Value,
                match.Player1.UserId.Value,
                match.Player2.UserId.Value,
                p1Position,
                p2Position,
                match.Player1.Champion,
                match.Player2.Champion,
                p1Drawn,
                p2Drawn
            );

            return init;
        }
    }