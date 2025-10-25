using api.Effect.DTOs;
using api.Effect.Services;
using Microsoft.AspNetCore.Mvc;
using VortexTCG.Common.DTO;

namespace api.Effect.Controller
{
    [ApiController]
    [Route("api/effect_description")]
    public class EffectDescriptionController : ControllerBase
    {
        private readonly EffectDescriptionService _service;
        public EffectDescriptionController(EffectDescriptionService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> list(CancellationToken ct) =>
            toActionResult(await _service.listAsync(ct));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> get(Guid id, CancellationToken ct) =>
            toActionResult(await _service.getAsync(id, ct));

        [HttpPost]
        public async Task<IActionResult> create([FromBody] EffectDescriptionInputDTO dto, CancellationToken ct) =>
            toActionResult(await _service.createAsync(dto, ct));

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> update(Guid id, [FromBody] EffectDescriptionInputDTO dto, CancellationToken ct) =>
            toActionResult(await _service.updateAsync(id, dto, ct));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> delete(Guid id, CancellationToken ct) =>
            toActionResult(await _service.deleteAsync(id, ct));

        private IActionResult toActionResult<T>(ResultDTO<T> result) =>
            StatusCode(result.statusCode, result);
    }       
}