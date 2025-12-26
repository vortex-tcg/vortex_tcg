using System;

namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class PlayCardResponseDto
    {
        public PlayCardPlayerResultDto PlayerResult { get; set; }
        public PlayCardOpponentResultDto OpponentResult { get; set; }
    }

    [Serializable]
    public class PlayCardPlayerResultDto
    {
        public Guid PlayerId { get; set; }
        public GameCardDto PlayedCard { get; set; }
        public PlayCardChampionDto Champion { get; set; }
        public int location { get; set; }
        public bool canPlayed { get; set; }
    }

    [Serializable]
    public class PlayCardOpponentResultDto
    {
        public Guid PlayerId { get; set; }
        public GameCardDto PlayedCard { get; set; }
        public PlayCardChampionDto Champion { get; set; }
        public int location { get; set; }
    }

    [Serializable]
    public class PlayCardChampionDto
    {
        public Guid Id { get; set; }
        public int Hp { get; set; }
        public int Gold { get; set; }
        public int SecondaryCurrency { get; set; }
    }

    [Serializable]
    public class GameCardDto
    {
        public int GameCardId { get; set; }
        public string Name { get; set; }
        public int Hp { get; set; }
        public int Attack { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; }
        public CardType CardType { get; set; }
    }
}