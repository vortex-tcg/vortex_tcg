using api.Effect.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Effect.DTOs
{
    public class EffectTypeDtoTests
    {
        [Fact]
        public void EffectTypeDto_CanSetAndGetProperties()
        {
            var id = Guid.NewGuid();
            var dto = new EffectTypeDto
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
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var dto1 = new EffectTypeDto { Id = id1, Label = "Burn" };
            var dto2 = new EffectTypeDto { Id = id2, Label = "Poison" };

            Assert.NotEqual(dto1.Id, dto2.Id);
            Assert.NotEqual(dto1.Label, dto2.Label);
        }

        [Fact]
        public void EffectTypeCreateDto_CanSetAndGetLabel()
        {
            var dto = new EffectTypeCreateDto { Label = "Freeze" };

            Assert.Equal("Freeze", dto.Label);
        }

        [Fact]
        public void EffectTypeCreateDto_HasRequiredValidation()
        {
            var dto = new EffectTypeCreateDto();
            // The [Required, MinLength(1)] attributes should be present on the property
            var property = typeof(EffectTypeCreateDto).GetProperty("Label");
            var attributes = property!.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
            
            Assert.NotEmpty(attributes);
        }

        [Fact]
        public void EffectTypeUpdateDto_CanSetAndGetLabel()
        {
            var dto = new EffectTypeUpdateDto { Label = "Stun" };

            Assert.Equal("Stun", dto.Label);
        }

        [Fact]
        public void EffectTypeUpdateDto_HasRequiredValidation()
        {
            var dto = new EffectTypeUpdateDto();
            // The [Required, MinLength(1)] attributes should be present on the property
            var property = typeof(EffectTypeUpdateDto).GetProperty("Label");
            var attributes = property!.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
            
            Assert.NotEmpty(attributes);
        }

        [Fact]
        public void EffectTypeCreateAndUpdateDto_AreConsistent()
        {
            var createDto = new EffectTypeCreateDto { Label = "TestLabel" };
            var updateDto = new EffectTypeUpdateDto { Label = createDto.Label };

            Assert.Equal(createDto.Label, updateDto.Label);
        }
    }
}
