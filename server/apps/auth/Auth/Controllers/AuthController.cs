using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace VortexTCG.Auth.Controllers;

public class AuthController(VortexDbContext db, IConfiguration configuration) : ControllerBase
{

}