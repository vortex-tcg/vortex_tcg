using api.Effect.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Effect.DTOs
{
    public class EffectDescriptionDtoTests
    {
        [Fact]
        public void EffectDescriptionInputDto_CanSetAndGetProperties()
        {
            EffectDescriptionInputDto dto = new EffectDescriptionInputDto
            {
                Label = "Test Label",
                Description = "Test Description",
                Parameter = "test param"
            };

            Assert.Equal("Test Label", dto.Label);
            Assert.Equal("Test Description", dto.Description);
            Assert.Equal("test param", dto.Parameter);
        }

        [Fact]
        public void EffectDescriptionInputDto_Parameter_CanBeNull()
        {
            EffectDescriptionInputDto dto = new EffectDescriptionInputDto
            {
                Label = "Test",
                Description = "Desc",
                Parameter = null
            };

            Assert.Null(dto.Parameter);
        }

        [Fact]
        public void EffectDescriptionInputDto_InitializedWithDefaults()
        {
            EffectDescriptionInputDto dto = new EffectDescriptionInputDto();

            Assert.Equal("", dto.Label);
            Assert.Equal("", dto.Description);
            Assert.Null(dto.Parameter);
        }

        [Fact]
        public void EffectDescriptionDto_CanSetAndGetProperties()
        {
            Guid id = Guid.NewGuid();
            EffectDescriptionDto dto = new EffectDescriptionDto
            {
                Id = id,
                Label = "Effect Label",
                Description = "Effect Description",
                Parameter = "param123"
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Effect Label", dto.Label);
            Assert.Equal("Effect Description", dto.Description);
            Assert.Equal("param123", dto.Parameter);
        }

        [Fact]
        public void EffectDescriptionDto_Parameter_CanBeNull()
        {
            EffectDescriptionDto dto = new EffectDescriptionDto
            {
                Id = Guid.NewGuid(),
                Label = "Test",
                Description = "Desc",
                Parameter = null
            };

            Assert.Null(dto.Parameter);
        }

        [Fact]
        public void EffectDescriptionDto_AllPropertiesAreIndependent()
        {
            EffectDescriptionDto dto1 = new EffectDescriptionDto { Id = Guid.NewGuid(), Label = "Label1", Description = "Desc1" };
            EffectDescriptionDto dto2 = new EffectDescriptionDto { Id = Guid.NewGuid(), Label = "Label2", Description = "Desc2" };

            Assert.NotEqual(dto1.Id, dto2.Id);
            Assert.NotEqual(dto1.Label, dto2.Label);
            Assert.NotEqual(dto1.Description, dto2.Description);
        }
    }
}
