using VortexTCG.Api.Rank.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Rank.DTOs
{
    public class RankDtoTests
    {
        [Fact]
        public void RankDTO_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();

            RankDTO dto = new RankDTO
            {
                Id = id,
                Label = "Diamond",
                nbVictory = 100
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Diamond", dto.Label);
            Assert.Equal(100, dto.nbVictory);
        }

        [Fact]
        public void RankDTO_DefaultValues_AreCorrect()
        {
            RankDTO dto = new RankDTO();

            Assert.Equal(string.Empty, dto.Label);
            Assert.Equal(0, dto.nbVictory);
        }

        [Fact]
        public void RankCreateDTO_CanSetAndGetAllProperties()
        {
            RankCreateDTO dto = new RankCreateDTO
            {
                Label = "Platinum",
                nbVictory = 50
            };

            Assert.Equal("Platinum", dto.Label);
            Assert.Equal(50, dto.nbVictory);
        }
    }
}