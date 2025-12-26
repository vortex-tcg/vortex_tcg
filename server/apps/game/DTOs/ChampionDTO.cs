namespace VortexTCG.Game.DTO {

    public class PlayCardChampionDto {
        public Guid Id { get; set; }

        public int Hp { get; set; }

        public int Gold { get; set; }

        public int SecondaryCurrency { get; set; }
    }

    public class BattleChampionDto {
        public int Hp { get; set; }
        public int SecondaryCurrency { get; set; }
        public int Gold { get; set; }
    }

}