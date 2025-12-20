namespace VortexTCG.Auth.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Bienvenue sur l’API VortexTCG",
                service = "VortexTCG.Auth",
                status = "ok",
                utc = DateTime.UtcNow
            });
        }
    }
}
