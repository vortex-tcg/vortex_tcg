using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.Auth.Providers
{
    public class UserProvider
    {
        private readonly VortexDbContext _db;
        public UserProvider(VortexDbContext db)
        {
            _db = db;
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
            => _db.Users.AnyAsync(u => u.Email == email, ct);

        public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
            => _db.Users.AnyAsync(u => u.Username == username, ct);
        public void AddUser(User user)
            => _db.Users.Add(user);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
