using Xunit;

namespace Tests
{
    public class BasicTests
    {
        [Fact]
        public void Test_Should_Always_Pass()
        {
            // Test qui réussit toujours - parfait pour tester la CI
            Assert.True(true);
        }

        [Fact]
        public void Test_Simple_Math()
        {
            // Test simple pour vérifier que xUnit fonctionne
            int result = 2 + 2;
            Assert.Equal(4, result);
        }

        [Theory]
        [InlineData(1, 1, 2)]
        [InlineData(5, 3, 8)]
        [InlineData(0, 0, 0)]
        public void Test_Addition_Multiple_Values(int a, int b, int expected)
        {
            // Test avec plusieurs valeurs - montre que xUnit fonctionne bien
            int result = a + b;
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Test_String_Contains()
        {
            // Test de string
            string message = "Hello World from xUnit";
            Assert.Contains("xUnit", message);
        }
    }
}