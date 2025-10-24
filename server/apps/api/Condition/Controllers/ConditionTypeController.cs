using Microsoft.AspNetCore.Mvc;
using VortexTCG.API.Service;
using VortexTCG.API.DTO;
using VortexTCG.DataAccess;
using VortexTCG.Common.DTO;

namespace VortexTCG.API.Controller
{
    [ApiController]
    [Route("api/condition_type/")]
    public class ConditionTypeController : ControllerBase
    {
        private readonly VortexDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ConditionTypeService _condition_type_service;

        public ConditionTypeController(VortexDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _condition_type_service = new ConditionTypeService(_db, _configuration);
        }

        [HttpPost("create")]
        public async Task<IActionResult> create([FromBody] ConditionTypeCreateDTO data)
        {
            ResultDTO<ConditionTypeCreateResponseDTO> result = await _condition_type_service.create(data);
            return StatusCode(result.statusCode, result);
        }
    }
}