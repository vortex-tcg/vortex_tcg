using System.Collections.Generic;
using System.Linq;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;

namespace game.Domaine.Match.Agregate;

public sealed class AttackHandler
{
    private List<AttackCard> _attackCards = new();

    public void ResetAttackHandler()
    {
        _attackCards = new List<AttackCard>();
    }

    public void RemoveAttackByPosition(int position)
    {
        _attackCards.RemoveAll(c => c.Position == position);
        ReorderAttackCards();
    }

    public void RemoveAttackByGameCardId(int gameCardId)
    {
        _attackCards.RemoveAll(c => c.GameCardId == gameCardId);
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

    public bool HasAttackAtPosition(int attackPosition)
    {
        return _attackCards.Any(a => a.Position == attackPosition);
    }

    public IReadOnlyList<AttackCard> GetAttackers()
        => _attackCards.ToList();

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