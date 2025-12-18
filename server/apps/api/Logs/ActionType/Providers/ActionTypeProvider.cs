
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using ActionTypeModel = VortexTCG.DataAccess.Models.ActionType;

namespace VortexTCG.Api.Logs.ActionType.Providers
{
	public class ActionTypeProvider
	{
		private readonly VortexDbContext _db;
		public ActionTypeProvider(VortexDbContext db)
		{
			_db = db;
		}

	public IQueryable<ActionTypeModel> Query() => _db.Actions.AsQueryable();

		public async Task<ActionTypeModel?> GetByIdAsync(Guid id)
		{
			return await _db.Actions.Include(a => a.Childs).Include(a => a.Parent).FirstOrDefaultAsync(a => a.Id == id);
		}

		public async Task AddAsync(ActionTypeModel actionType)
		{
			await _db.Actions.AddAsync(actionType);
			await _db.SaveChangesAsync();
		}

		public async Task UpdateAsync(ActionTypeModel actionType)
		{
			_db.Actions.Update(actionType);
			await _db.SaveChangesAsync();
		}

		public async Task DeleteAsync(ActionTypeModel actionType)
		{
			_db.Actions.Remove(actionType);
			await _db.SaveChangesAsync();
		}
	}
}
