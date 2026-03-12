using System.Collections.Generic;
using System.Linq;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;

namespace game.Domaine.Match.Agregate;

public sealed class AttackHandler
{
    private List<AttackCard> _attackCards = new();
    private List<DefenseCard> _defenseCards = new();

    public void ResetAttackHandler()
    {
        _attackCards = new List<AttackCard>();
        _defenseCards = new List<DefenseCard>();
    }

    public void RemoveAttackByPosition(int position)
    {
        _attackCards.RemoveAll(c => c.Position == position);
        _defenseCards.RemoveAll(d => d.AttackPosition == position);

        ReorderAttackCards();
    }

    public void RemoveAttackByGameCardId(int gameCardId)
    {
        AttackCard? removedAttack = _attackCards.FirstOrDefault(c => c.GameCardId == gameCardId);

        _attackCards.RemoveAll(c => c.GameCardId == gameCardId);

        if (removedAttack != null)
        {
            _defenseCards.RemoveAll(d => d.AttackPosition == removedAttack.Position);
        }

        ReorderAttackCards();
    }

    public void AddAttack(int position, int gameCardId)
    {
        bool alreadyExists = _attackCards.Any(c => c.Position == position || c.GameCardId == gameCardId);
        if (alreadyExists)
            return;

        _attackCards.Add(new AttackCard
        {
            Position = position,
            GameCardId = gameCardId
        });

        ReorderAttackCards();
    }

    public bool IsEngaged(int position)
    {
        return _attackCards.Any(c => c.Position == position);
    }

    public IReadOnlyList<AttackCard> GetAttacker()
        => _attackCards.ToList();

    public IReadOnlyList<DefenseCard> GetDefender()
        => _defenseCards.ToList();

    public DefenseCard GetSpecificDefender(int attackPosition)
        => _defenseCards.Single(defender => defender.AttackPosition == attackPosition);

    public void RemoveDefenseByGameCardId(int gameCardId)
    {
        _defenseCards.RemoveAll(defender => defender.GameCardId == gameCardId);
    }

    public void RemoveDefenseByPosition(int position)
    {
        _defenseCards.RemoveAll(defender => defender.Position == position);
    }

    public void AddDefense(int position, int gameCardId, int attackPosition)
    {
        _defenseCards.RemoveAll(defenseCard => defenseCard.AttackPosition == attackPosition);

        _defenseCards.Add(new DefenseCard
        {
            Position = position,
            GameCardId = gameCardId,
            AttackPosition = attackPosition
        });
    }

    public AttackOrderUpdatedDto FormatAttackResponseDto()
    {
        return new AttackOrderUpdatedDto
        {
            EngagedCards = _attackCards
                .Select(c => new EngagedCardDto
                {
                    Position = c.Position,
                    GameCardId = c.GameCardId,
                    AttackOrder = c.AttackOrder
                })
                .ToList()
        };
    }

    private void ReorderAttackCards()
    {
        for (int i = 0; i < _attackCards.Count; i++)
        {
            _attackCards[i].AttackOrder = i + 1;
        }
    }
}