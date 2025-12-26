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

}