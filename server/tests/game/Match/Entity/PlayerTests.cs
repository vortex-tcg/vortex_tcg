using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;

namespace game.Tests.Domain.Entity;

public class PlayerTests
{
    [Fact]
    public void Player_ExposesAssignedDeckId()
    {
        DeckId deckId = new DeckId(Guid.NewGuid());
        Player player = new Player(
            new UserId(Guid.NewGuid()),
            deckId,
            new PlayerDeck(Array.Empty<GameCardDto>()),
            MatchHelpers.MakeChampion()
        );

        Assert.Equal(deckId, player.DeckId);
    }
}
