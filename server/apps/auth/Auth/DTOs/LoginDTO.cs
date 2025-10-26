using VortexTCG.DataAccess.Models;

namespace VortexTCG.Auth.DTOs
{

    public class LoginDTO
    {
        public string email { get; set; } = "";
        public string password { get; set; } = "";
    }

    public class LoginResponseDTO
    {
        public Guid id { get; set; }
        public string username { get; set; } = "";
        public string token { get; set; } = "";
        public string role { get; set; } = "USER";
    }

    public class LoginUserDTO
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public Role Role { get; set; } = Role.USER;
    }
}