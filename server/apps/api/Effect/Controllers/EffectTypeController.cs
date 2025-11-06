using api.Effect.DTOs;
using api.Effect.Services;
using Microsoft.AspNetCore.Mvc;
using VortexTCG.Common.DTO;


namespace api.Effect.Controller
{
    [ApiController]
    [Route("api/effect_type")]
    public class EffectTypeController : ControllerBase
    {
        private readonly EffectTypeService _service;
        public EffectTypeController(EffectTypeService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> List(CancellationToken ct) =>
            ToActionResult(await _service.ListAsync(ct));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
            ToActionResult(await _service.GetAsync(id, ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EffectTypeCreateDTO dto, CancellationToken ct) =>
            ToActionResult(await _service.CreateAsync(dto, ct));

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] EffectTypeUpdateDTO dto, CancellationToken ct) =>
            ToActionResult(await _service.UpdateAsync(id, dto, ct));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
            ToActionResult(await _service.DeleteAsync(id, ct));

        private IActionResult ToActionResult<T>(ResultDTO<T> result) =>
            StatusCode(result.statusCode, result);
    }
}