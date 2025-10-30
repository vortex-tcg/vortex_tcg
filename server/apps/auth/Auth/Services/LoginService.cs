using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Auth.DTOs;
using VortexTCG.Auth.Providers;
using VortexTCG.Common.DTO;
using System.Text;
using Scrypt;

namespace VortexTCG.Auth.Services {

    public class LoginService
    {

        private readonly VortexDbContext _db;
        private readonly IConfiguration _configuration;

        private readonly LoginProvider _provider;

        public LoginService(VortexDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _provider = new LoginProvider(_db);
        }

        private Boolean checkIsEmptyLoginData(LoginDTO data)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.email) ||
                string.IsNullOrWhiteSpace(data.password))
            {
                return true;
            }
            return false;
        }

        private JwtSecurityToken generateAccessToken(string userName)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName)
            };

            var secretKey = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT SecretKey is not configured.");

            byte[] key = Encoding.UTF8.GetBytes(secretKey);

            return new JwtSecurityToken(
                claims: claims,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );
        }

        private string convertRoleToString(Role role)
        {
            switch (role)
            {
                case Role.USER:
                    return "USER";
                case Role.ADMIN:
                    return "ADMIN";
                case Role.SUPER_ADMIN:
                    return "SUPER_ADMIN";
                default:
                    return "UNKNOWN";
            }
        }

        public async Task<ResultDTO<LoginResponseDTO>> login(LoginDTO data)
        {
            if (checkIsEmptyLoginData(data))
            {
                return new ResultDTO<LoginResponseDTO>
                {
                    success = false,
                    statusCode = 400,
                    message = "Email ou mot de passe sont requis."
                };
            }

            LoginUserDTO? user = await _provider.getFirstUserByEmail(data.email);

            if (user == null)
            {
                return new ResultDTO<LoginResponseDTO>
                {
                    success = false,
                    statusCode = 401,
                    message = "Invalid credentials."
                };
            }

            ScryptEncoder encoder = new ScryptEncoder();

            try
            {
                if (!encoder.Compare(data.password, user.Password))
                {
                    return new ResultDTO<LoginResponseDTO>
                    {
                        success = false,
                        statusCode = 401,
                        message = "Invalid credentials."
                    };
                }
            }
            catch
            {
                return new ResultDTO<LoginResponseDTO>
                {
                    success = false,
                    statusCode = 401,
                    message = "Invalid credentials."
                };
            }

            JwtSecurityToken token = generateAccessToken(user.Username);

            return new ResultDTO<LoginResponseDTO>
            {
                success = true,
                statusCode = 200,
                data = new LoginResponseDTO
                {
                    id = user.Id,
                    username = user.Username,
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    role = convertRoleToString(user.Role)
                }
            };
        }
    }
}