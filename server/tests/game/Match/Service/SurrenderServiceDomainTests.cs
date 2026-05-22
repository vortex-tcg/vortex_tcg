using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Domain.Service;

public class SurrenderServiceDomainTests
{
    [Fact]
    public void Apply_Throws_WhenMatchIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            SurrenderService.Apply(null!, MatchHelpers.MakePlayer()));
    }

    [Fact]
    public void Apply_Throws_WhenPlayerIsNull()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();

        Assert.Throws<ArgumentNullException>(() =>
            SurrenderService.Apply(match, null!));
    }
}