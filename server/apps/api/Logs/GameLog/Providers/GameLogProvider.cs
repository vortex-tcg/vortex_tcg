using GameLogModel = VortexTCG.DataAccess.Models.Gamelog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.Api.Logs.GameLog.Providers
{
	public class GameLogProvider
	{
		private readonly VortexDbContext _db;
		public GameLogProvider(VortexDbContext db)
		{
			_db = db;
		}

	public IQueryable<GameLogModel> Query() => _db.Gamelogs.AsQueryable();

		public async Task<GameLogModel?> GetByIdAsync(Guid id)
		{
			return await _db.Gamelogs.Include(g => g.User).Include(g => g.Actions).FirstOrDefaultAsync(g => g.Id == id);
		}

		public async Task AddAsync(GameLogModel gamelog)
		{
			await _db.Gamelogs.AddAsync(gamelog);
			await _db.SaveChangesAsync();
		}

		public async Task UpdateAsync(GameLogModel gamelog)
		{
			_db.Gamelogs.Update(gamelog);
			await _db.SaveChangesAsync();
		}

		public async Task DeleteAsync(GameLogModel gamelog)
		{
			_db.Gamelogs.Remove(gamelog);
			await _db.SaveChangesAsync();
		}
	}
}
