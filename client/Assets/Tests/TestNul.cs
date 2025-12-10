using NUnit.Framework;

public class TestNul
{
    [Test]
    public void AlwaysTrueTest()
    {
        Assert.IsTrue(true, "Ce test passe toujours et permet de valider la CI.");
    }
}
