namespace game.Domaine.Match.DTO;
public sealed class BattleResolveDTOs
{
    public List<CardBattleResultDto> Battles { get; set; } = new();
    public List<DirectChampionDamageDto> DirectChampionDamages { get; set; } = new();
    public List<int> DeadCardIds { get; set; } = new();
    public int CurrentPlayerChampionHp { get; set; }
    public int OpponentPlayerChampionHp { get; set; }
}

public sealed class CardBattleResultDto
{
    public int AttackerCardId { get; set; }
    public int AttackerPosition { get; set; }
    public int DefenderCardId { get; set; }
    public int DefenderPosition { get; set; }

    public int DamageToAttacker { get; set; }
    public int DamageToDefender { get; set; }

    public int AttackerRemainingHp { get; set; }
    public int DefenderRemainingHp { get; set; }

    public bool AttackerDied { get; set; }
    public bool DefenderDied { get; set; }
}

public sealed class DirectChampionDamageDto
{
    public int AttackerCardId { get; set; }
    public int AttackerPosition { get; set; }
    public int Damage { get; set; }
    public int ChampionRemainingHp { get; set; }
}