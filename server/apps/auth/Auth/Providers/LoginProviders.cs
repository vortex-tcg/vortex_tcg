using Microsoft.EntityFrameworkCore;
using VortexTCG.Auth.DTOs;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.Auth.Providers {

    public class LoginProvider
    {
        private readonly VortexDbContext _db;

        public LoginProvider(VortexDbContext db)
        {
            _db = db;
        }

        public async Task<LoginUserDTO> getFirstUserByEmail(string email)
        {
            return await _db.Users
                            .Where(u => u.Email == email)
                            .Select(u => new LoginUserDTO
                            {
                                Id = u.Id,
                                Username = u.Username,
                                Password = u.Password,
                                Role = u.Role
                            })
                            .SingleOrDefaultAsync();
        }
    }
}