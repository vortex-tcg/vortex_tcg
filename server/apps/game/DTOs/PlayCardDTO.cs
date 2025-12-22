// using VortexTCG.Game.DTO;

namespace VortexTCG.Game.DTO
{

    public class PlayCardOpponentResultDto {
        public Guid PlayerId { get; set; }
        public GameCardDto PlayedCard { get; set; }
        public PlayCardChampionDto Champion { get; set; }
        public int location { get; set; }
    }

    public class PlayCardPlayerResultDto {
        public Guid PlayerId { get; set; }
        public GameCardDto PlayedCard { get; set; }
        public PlayCardChampionDto Champion { get; set; }
        public int location { get; set; }
        public bool canPlayed { get; set; }
    }

    public class PlayCardResponseDto {
        public PlayCardPlayerResultDto PlayerResult { get; set; }
        public PlayCardOpponentResultDto OpponentResult { get; set; }
    }

}