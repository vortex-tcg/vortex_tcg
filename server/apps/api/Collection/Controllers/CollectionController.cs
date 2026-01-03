using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Collection.DTOs;
using VortexTCG.Api.Collection.Services;
using VortexTCG.Common.Services;
using VortexTCG.Common.DTO;

namespace VortexTCG.Api.Collection.Controllers
{
    [ApiController]
    [Route("api/collection")]
    public class CollectionController : VortexBaseController
    {
        private readonly CollectionService _service;
        public CollectionController(CollectionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        => toActionResult<CollectionDto[]>(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        => toActionResult(await _service.GetByIdAsync(id));

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCollectionByUserId(Guid userId)
        => toActionResult(await _service.GetCollectionByUserId(userId));

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CollectionCreateDto dto)
        => toActionResult(await _service.CreateAsync(dto));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CollectionCreateDto dto)
        => toActionResult(await _service.UpdateAsync(id, dto));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        => toActionResult(await _service.DeleteAsync(id));
    }
}
