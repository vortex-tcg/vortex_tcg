using VortexTCG.Common.DTO;
using Xunit;

namespace VortexTCG.Tests.Shared.Common.DTOs
{
    public class ReturnTypeDtoTests
    {
        [Fact]
        public void ResultDTO_CanSetAndGetAllProperties()
        {
            ResultDTO<string> dto = new ResultDTO<string>
            {
                success = true,
                statusCode = 200,
                message = "OK",
                data = "some data"
            };

            Assert.True(dto.success);
            Assert.Equal(200, dto.statusCode);
            Assert.Equal("OK", dto.message);
            Assert.Equal("some data", dto.data);
        }

        [Fact]
        public void ResultDTO_DefaultValues_AreCorrect()
        {
            ResultDTO<int> dto = new ResultDTO<int>();

            Assert.False(dto.success);
            Assert.Equal(0, dto.statusCode);
            Assert.Null(dto.message);
            Assert.Equal(0, dto.data);
        }

        [Fact]
        public void ResultDTO_SupportsNullableData()
        {
            ResultDTO<string?> dto = new ResultDTO<string?> { success = false, statusCode = 404, data = null };

            Assert.Null(dto.data);
            Assert.False(dto.success);
        }

        [Fact]
        public void ResultDTO_WorksWithComplexType()
        {
            ResultDTO<List<int>> dto = new ResultDTO<List<int>>
            {
                success = true,
                statusCode = 200,
                data = new List<int> { 1, 2, 3 }
            };

            Assert.Equal(3, dto.data!.Count);
        }
    }
}