using System;
using Microsoft.AspNetCore.Mvc;
using VortexTCG.Auth.Controllers;
using Xunit;

namespace VortexTCG.Tests.Auth.Controllers
{
    public class RootControllerTest
    {
        [Fact]
        public void Get_ReturnsOk_WithPayload()
        {
            RootController controller = new RootController();

            IActionResult result = controller.Get();
            OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }
    }
}
