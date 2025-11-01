using Microsoft.EntityFrameworkCore;
using VortexTCG.Class.DTOs;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.Class.Providers {

    public class ClassProvider
    {
        private readonly VortexDbContext _db;

        public ClassProvider(VortexDbContext db)
        {
            _db = db;
        }

        public async Task<ClassDTO> getFirstUserClassByGUID(string label)
        {
            return await _db.Class
                            .Where(u => u.Label == label)
                            .Select(u => new ClassDTO
                            {
                                Id = u.Id,
                                Label = u.Label
                            })
                            .SingleOrDefaultAsync();
        }
    }
}