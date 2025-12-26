using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Services;
using VortexTCG.Common.Services;
using VortexTCG.Common.DTO;

namespace VortexTCG.Api.Deck.Controllers
{
    [ApiController]
    [Route("api/deck")]
    public class DeckController : VortexBaseController
    {
        private readonly DeckService _service;

        public DeckController(DeckService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        => toActionResult<DeckDto[]>(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        => toActionResult(await _service.GetByIdAsync(id));

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        => toActionResult<DeckDto[]>(await _service.GetByUserIdAsync(userId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DeckCreateDto dto)
        => toActionResult(await _service.CreateAsync(dto));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DeckUpdateDto dto)
        => toActionResult(await _service.UpdateAsync(id, dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        => toActionResult(await _service.DeleteAsync(id));
    }
}
