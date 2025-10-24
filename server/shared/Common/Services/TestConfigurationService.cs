using Microsoft.Extensions.Configuration;

namespace VortexTCG.Common.Services
{
    public class TestConfigurationBuilder
    {
        static public IConfiguration getTestConfiguration()
        {
            Dictionary<string, string> inMemorySettings = new Dictionary<string, string>
            {
                { "JwtSettings:SecretKey", "123soleiljspjesaispaaaaaaaaaahahahahahhahahah" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }
    }
}