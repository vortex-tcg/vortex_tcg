using Microsoft.AspNetCore.Mvc;

namespace auth.Controllers;

[ApiController]
[Route("api/auth/[controller]")]

public class LoginController : ControllerBase
{
    public class LoginData
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    
    [HttpGet]
    public IActionResult GetLoginInfo()
    {
        var response = new
        {
            Message = "Salut ! Je suis ton API de connexion",
            HeureServeur = DateTime.Now,
            StatutServeur = "En ligne"
        };
        return Ok(response);
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginData data)
    {
        if (data == null)
        {
            return BadRequest("No data provided");
        }
        else
        {
            return Ok(data);
        }
    }
}
