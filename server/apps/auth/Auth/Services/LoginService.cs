using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Auth.DTOs;
using VortexTCG.Auth.Providers;
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

        private Boolean checkLoginData(LoginDTO data)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.email) ||
                string.IsNullOrWhiteSpace(data.password))
            {
                return false;
            }
            return true;
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
            switch(role)
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

        public async Task<LoginResponseDTO> login(LoginDTO data)
        {
            if (!checkLoginData(data))
            {
                throw new Exception("BAD_REQUEST");
            }

            LoginUserDTO user = await _provider.getFirstUserByEmail(data.email);

            if (user == null)
            {
                throw new Exception("UNAUTHORIZED");
            }

            ScryptEncoder encoder = new ScryptEncoder();

            try
            {
                encoder.Compare(data.password, user.Password);
            }
            catch
            {
                throw new Exception("UNAUTHORIZED");
            }

            JwtSecurityToken token = generateAccessToken(user.Username);

            return new LoginResponseDTO
            {
                id = user.Id,
                username = user.Username,
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role = convertRoleToString(user.Role)
            };
        }
    }
}