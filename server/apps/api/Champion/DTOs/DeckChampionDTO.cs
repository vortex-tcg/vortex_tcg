namespace VortexTCG.Api.Champion.DTOs {

    public class DeckChampionDto {

        public Guid ChampionID { get; set; }

        public string Name { get; set; } = default!;

        public string Description { get; set; } = default!;

        public int HP { get; set; } = default!;

        public string Picture { get; set; } = default!;

        public Guid FactionId { get; set; } = default!;

    }
}