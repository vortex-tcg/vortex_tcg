namespace VortexTCG.Game.DTO {

    public class AttackResponseDto {
        public List<int> AttackCardsId { get; set; }
        public Guid PlayerId { get; set; }
        public Guid OpponentId { get; set; }
    }

    public class DefenseCardDataDto {
        public int cardId { get; set; }
        public int opponentCardId { get; set; }
    }

    public class DefenseDataResponseDto {
        public List<int> AttackCardsId { get; set; }
        public List<DefenseCardDataDto> DefenseCards { get; set; }
    }

    public class DefenseResponseDto {
        public DefenseDataResponseDto data { get; set; }
        public Guid PlayerId { get; set; }
        public Guid OpponentId { get; set; }
    }

    public class BattleAgainstCardDataDto {
        public bool isAttackerDead { get; set; }
        public bool isDefenderDead { get; set; }

        public int attackerDamageDeal { get; set; }
        public int defenderDamageDeal { get; set; }

        public GameCardDto attackerCard { get; set; }
        public GameCardDto defenderCard { get; set; }

        public BattleChampionDto attackerChamp { get; set; }
        public BattleChampionDto defenderChamp { get; set; }
    }

    public class BattlaAgainstChampDataDto {
        public bool isChampDead { get; set; }
        public bool isCardDead { get; set; }

        public int attackerDamageDeal { get; set; }
        public int championDamageDeal { get; set; }

        public GameCardDto attackerCard { get; set; }

        public BattleChampionDto attackerChamp { get; set; }
        public BattleChampionDto defenderChamp { get; set; }
    }

    public class BattleDataDto {
        public bool isAgainstChamp { get; set; }
        public BattleAgainstCardDataDto? againstCard { get; set; }
        public BattlaAgainstChampDataDto? againstChamp { get; set; }
    }

    public class BattlesDataDto {
        public List<BattleDataDto> battles { get; set; }
    }

    public class BattleResponseDto {
        public BattlesDataDto data { get; set; }
        public Guid Player1Id { get; set; }
        public Guid Player2Id { get; set; }
    }

}