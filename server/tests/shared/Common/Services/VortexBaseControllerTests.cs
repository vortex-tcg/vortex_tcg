using Microsoft.AspNetCore.Mvc;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;
using Xunit;

namespace VortexTCG.Tests.Shared.Common.Services
{
    public class VortexBaseControllerTests
    {
        private class TestController : VortexBaseController
        {
            public IActionResult InvokeToActionResult<T>(ResultDTO<T> result) => toActionResult(result);
        }

        [Fact]
        public void ToActionResult_Returns_CorrectStatusCode()
        {
            TestController controller = new TestController();
            ResultDTO<string> result = new ResultDTO<string> { success = true, statusCode = 200, data = "ok" };

            IActionResult actionResult = controller.InvokeToActionResult(result);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(200, objectResult.StatusCode);
            Assert.Equal(result, objectResult.Value);
        }

        [Fact]
        public void ToActionResult_Returns_404_WhenNotFound()
        {
            TestController controller = new TestController();
            ResultDTO<string> result = new ResultDTO<string> { success = false, statusCode = 404, message = "Not found" };

            IActionResult actionResult = controller.InvokeToActionResult(result);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public void ToActionResult_Returns_201_WhenCreated()
        {
            TestController controller = new TestController();
            ResultDTO<int> result = new ResultDTO<int> { success = true, statusCode = 201, data = 42 };

            IActionResult actionResult = controller.InvokeToActionResult(result);

            ObjectResult objectResult = Assert.IsType<ObjectResult>(actionResult);
            Assert.Equal(201, objectResult.StatusCode);
        }
    }
}