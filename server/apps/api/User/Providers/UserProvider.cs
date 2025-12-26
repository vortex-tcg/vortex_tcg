using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using UserModel = VortexTCG.DataAccess.Models.User;

namespace VortexTCG.Api.User.Providers
{
	public class UserProvider
	{
		private readonly VortexDbContext _db;
		public UserProvider(VortexDbContext db)
		{
			_db = db;
		}

		public IQueryable<UserModel> Query() => _db.Users.AsQueryable();

		public async Task<UserModel?> GetByIdAsync(Guid id)
		{
			return await _db.Users.Include(u => u.Rank).FirstOrDefaultAsync(u => u.Id == id);
		}

		public async Task<UserModel?> GetByUsernameAsync(string username)
		{
			return await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
		}

		public async Task AddAsync(UserModel user)
		{
			await _db.Users.AddAsync(user);
			await _db.SaveChangesAsync();
		}

		public async Task UpdateAsync(UserModel user)
		{
			_db.Users.Update(user);
			await _db.SaveChangesAsync();
		}

		public async Task DeleteAsync(UserModel user)
		{
			_db.Users.Remove(user);
			await _db.SaveChangesAsync();
		}
	}
}
