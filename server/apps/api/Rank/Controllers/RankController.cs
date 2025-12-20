using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Rank.DTOs;
using VortexTCG.Api.Rank.Services;
using VortexTCG.Common.Services;
using VortexTCG.Common.DTO;

namespace VortexTCG.Api.Rank.Controllers
{
    [ApiController]
    [Route("api/rank")]
    public class RankController : VortexBaseController
    {
        private readonly RankService _service;
        public RankController(RankService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return toActionResult<RankDTO[]>(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return toActionResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] RankCreateDTO dto)
        {
            var result = await _service.CreateAsync(dto);
            return toActionResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RankCreateDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return toActionResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            return toActionResult(result);
        }
    }
}
