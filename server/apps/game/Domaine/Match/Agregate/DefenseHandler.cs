namespace game.Domaine.Match.Agregate;

using System.Collections.Generic;
using System.Linq;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;
public sealed class DefenseHandler
{
    private List<DefenseCard> _defenseCards = new();

    public void ResetDefenseHandler()
    {
        _defenseCards = new List<DefenseCard>();
    }

    public IReadOnlyList<DefenseCard> GetDefenders()
        => _defenseCards.ToList();

    public bool IsDefenseEngaged(int defensePosition)
    {
        return _defenseCards.Any(d => d.Position == defensePosition);
    }

    public bool IsDefenseEngagedOnAttack(int defensePosition, int attackPosition)
    {
        return _defenseCards.Any(d => d.Position == defensePosition && d.AttackPosition == attackPosition);
    }

    public bool HasDefenseOnAttack(int attackPosition)
    {
        return _defenseCards.Any(d => d.AttackPosition == attackPosition);
    }

    public DefenseCard? GetDefenseByDefensePosition(int defensePosition)
    {
        return _defenseCards.FirstOrDefault(d => d.Position == defensePosition);
    }

    public DefenseCard? GetDefenseByAttackPosition(int attackPosition)
    {
        return _defenseCards.FirstOrDefault(d => d.AttackPosition == attackPosition);
    }

    public void RemoveDefenseByPosition(int defensePosition)
    {
        _defenseCards.RemoveAll(d => d.Position == defensePosition);
    }

    public void RemoveDefenseByGameCardId(int gameCardId)
    {
        _defenseCards.RemoveAll(d => d.GameCardId == gameCardId);
    }

    public void RemoveDefenseByAttackPosition(int attackPosition)
    {
        _defenseCards.RemoveAll(d => d.AttackPosition == attackPosition);
    }

    public void AddOrReplaceDefense(int defensePosition, int gameCardId, int attackPosition)
    {
        _defenseCards.RemoveAll(d => d.Position == defensePosition);
        _defenseCards.RemoveAll(d => d.AttackPosition == attackPosition);

        _defenseCards.Add(new DefenseCard
        {
            Position = defensePosition,
            GameCardId = gameCardId,
            AttackPosition = attackPosition
        });
    }

    public DefenseUpdatedDto FormatDefenseResponseDto()
    {
        return new DefenseUpdatedDto
        {
            EngagedCards = _defenseCards
                .Select(d => new EngagedDefenseCardDto
                {
                    Position = d.Position,
                    GameCardId = d.GameCardId,
                    PositionOpponentCard = d.AttackPosition
                })
                .ToList()
        };
    }
}