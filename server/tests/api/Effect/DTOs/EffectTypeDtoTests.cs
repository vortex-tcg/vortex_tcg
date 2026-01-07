using api.Effect.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Effect.DTOs
{
    public class EffectTypeDtoTests
    {
        [Fact]
        public void EffectTypeDto_CanSetAndGetProperties()
        {
            Guid id = Guid.NewGuid();
            EffectTypeDto dto = new EffectTypeDto
            {
                Id = id,
                Label = "Burn"
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Burn", dto.Label);
        }

        [Fact]
        public void EffectTypeDto_AllPropertiesAreIndependent()
        {
            Guid id1 = Guid.NewGuid();
            Guid id2 = Guid.NewGuid();

            EffectTypeDto dto1 = new EffectTypeDto { Id = id1, Label = "Burn" };
            EffectTypeDto dto2 = new EffectTypeDto { Id = id2, Label = "Poison" };

            Assert.NotEqual(dto1.Id, dto2.Id);
            Assert.NotEqual(dto1.Label, dto2.Label);
        }

        [Fact]
        public void EffectTypeCreateDto_CanSetAndGetLabel()
        {
            EffectTypeCreateDto dto = new EffectTypeCreateDto { Label = "Freeze" };

            Assert.Equal("Freeze", dto.Label);
        }

        [Fact]
        public void EffectTypeCreateDto_HasRequiredValidation()
        {
            EffectTypeCreateDto dto = new EffectTypeCreateDto();
            // The [Required, MinLength(1)] attributes should be present on the property
            System.Reflection.PropertyInfo? property = typeof(EffectTypeCreateDto).GetProperty("Label");
            Assert.NotNull(property);
            object[] attributes = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
            
            Assert.NotEmpty(attributes);
        }

        [Fact]
        public void EffectTypeUpdateDto_CanSetAndGetLabel()
        {
            EffectTypeUpdateDto dto = new EffectTypeUpdateDto { Label = "Stun" };

            Assert.Equal("Stun", dto.Label);
        }

        [Fact]
        public void EffectTypeUpdateDto_HasRequiredValidation()
        {
            EffectTypeUpdateDto dto = new EffectTypeUpdateDto();
            // The [Required, MinLength(1)] attributes should be present on the property
            System.Reflection.PropertyInfo? property = typeof(EffectTypeUpdateDto).GetProperty("Label");
            Assert.NotNull(property);
            object[] attributes = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
            
            Assert.NotEmpty(attributes);
        }

        [Fact]
        public void EffectTypeCreateAndUpdateDto_AreConsistent()
        {
            EffectTypeCreateDto createDto = new EffectTypeCreateDto { Label = "TestLabel" };
            EffectTypeUpdateDto updateDto = new EffectTypeUpdateDto { Label = createDto.Label };

            Assert.Equal(createDto.Label, updateDto.Label);
        }
    }
}
