using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Class.Services;
using VortexTCG.Class.DTOs;
using VortexTCG.Common.DTO;

namespace VortexTCG.Class.Controllers
{

    [ApiController]
    [Route("api/class")]
    public class ClassController : ControllerBase
    {
        private readonly VortexDbContext _db;
        private readonly IConfiguration _configuration;

        private readonly ClassService _class_service;

        public ClassController(VortexDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _class_service = new ClassService(_db, _configuration);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ClassCreateDTO data)
        {
            ResultDTO<ClassDTO> result = await _class_service.CreateClass(data);
            return StatusCode(result.statusCode, result);
        }
        [HttpGet("{label}")]
        public async Task<IActionResult> Get(string label)
        {
            ResultDTO<ClassDTO> result = await _class_service.GetClass(label);
            return StatusCode(result.statusCode, result);
        }
        [HttpDelete("{label}")]
        public async Task<IActionResult> Delete(string label)
        {
            ResultDTO<ClassDTO> result = await _class_service.DeleteClass(label);
            return StatusCode(result.statusCode, result);
        }
        [HttpPut("update/{label}")]
        public async Task<IActionResult> Update(string label, [FromBody] ClassUpdateDTO data)
        {
            ResultDTO<ClassDTO> result = await _class_service.UpdateClass(label, data);
            return StatusCode(result.statusCode, result);
        }
    }
}