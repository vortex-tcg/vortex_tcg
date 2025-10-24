using VortexTCG.API.DTO;
using VortexTCG.API.Provider;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess;

namespace VortexTCG.API.Service
{
    public class ConditionTypeService
    {

        private readonly VortexDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ConditionTypeProvider _provider;

        public ConditionTypeService(VortexDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _provider = new ConditionTypeProvider(_db);
        }


        private bool checkIfCreateDataIsEmpty(ConditionTypeCreateDTO data)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.label)
            )
            {
                return true;
            }
            return false;
        }

        public async Task<ResultDTO<ConditionTypeCreateResponseDTO>> create(ConditionTypeCreateDTO data)
        {
            if (checkIfCreateDataIsEmpty(data))
            {
                return new ResultDTO<ConditionTypeCreateResponseDTO>
                {
                    statusCode = 400,
                    success = false,
                    message = "Le label est nécessaire."
                };
            }

            return new ResultDTO<ConditionTypeCreateResponseDTO>
            {
                statusCode = 201,
                success = true,
                message = "Le label est nécessaire."
            };
        }

    }
}