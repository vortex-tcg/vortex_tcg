using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;

namespace VortexTCG.Auth.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
	private readonly VortexDbContext _db;
	private readonly IConfiguration _configuration;

	public AuthController(VortexDbContext db, IConfiguration configuration)
	{
		_db = db;
		_configuration = configuration;
	}
}