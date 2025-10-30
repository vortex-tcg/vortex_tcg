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
        public async Task<ActionResult<List<RankDTO>>> GetAll()
        {
            var ranks = await _service.GetAllAsync();
            return Ok(ranks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RankDTO>> GetById(Guid id)
        {
            var rank = await _service.GetByIdAsync(id);
            if (rank == null) return NotFound();
            return Ok(rank);
        }

        [HttpPost]
        public async Task<ActionResult<ResultDTO<RankDTO>>> Add([FromBody] RankCreateDTO dto)
        {
            var result = await _service.CreateAsync(dto);
            if (!result.success)
                return StatusCode(result.statusCode, result);
            return CreatedAtAction(nameof(GetById), new { id = result.data!.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResultDTO<RankDTO>>> Update(Guid id, [FromBody] RankCreateDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (!result.success)
                return StatusCode(result.statusCode, result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResultDTO<object>>> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.success)
                return StatusCode(result.statusCode, result);
            return StatusCode(result.statusCode, result);
        }
    }
}
