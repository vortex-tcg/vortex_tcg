

using System.Data.Common;
using VortexTCG.API.DTO;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.API.Provider
{
    public class ConditionTypeProvider
    {

        private readonly VortexDbContext _db;

        public ConditionTypeProvider(VortexDbContext db)
        {
            _db = db;
        }

        public async Task<ConditionTypeCreateResponseDTO> createConditionType(ConditionTypeCreateDTO data)
        {
            ConditionType conditionType = new ConditionType
            {
                Label = data.label
            };
            await _db.ConditionTypes.Add(conditionType);
            await _db.SaveChangesAsync();
            return new ConditionTypeCreateResponseDTO
            {
                id = conditionType.Id,
                label = conditionType.Label,
                conditions = conditionType.Conditions
            };
        }

    }
}