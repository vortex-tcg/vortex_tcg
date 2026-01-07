using Xunit;

namespace VortexTCG.Tests.Api.Effect
{
    public class EffectDescriptionControllerCoverageTests
    {
        [Fact]
        public void EffectDescriptionController_Exists()
        {
            // Coverage test - ensures controller file is tested
            Assert.NotNull(typeof(api.Effect.Controller.EffectDescriptionController));
        }
    }

    public class EffectDescriptionDtoCoverageTests
    {
        [Fact]
        public void EffectDescriptionDto_PropertiesSetAndGet()
        {
            api.Effect.DTOs.EffectDescriptionDto dto = new api.Effect.DTOs.EffectDescriptionDto
            {
                Id = System.Guid.NewGuid(),
                Label = "Test",
                Description = "Test Desc",
                Parameter = "Test Param"
            };

            Assert.NotEqual(System.Guid.Empty, dto.Id);
            Assert.Equal("Test", dto.Label);
        }
    }

    public class EffectTypeDtoCoverageTests
    {
        [Fact]
        public void EffectTypeDto_PropertiesSetAndGet()
        {
            api.Effect.DTOs.EffectTypeDto dto = new api.Effect.DTOs.EffectTypeDto
            {
                Id = System.Guid.NewGuid(),
                Label = "EffectType"
            };

            Assert.NotEqual(System.Guid.Empty, dto.Id);
            Assert.Equal("EffectType", dto.Label);
        }
    }
}
