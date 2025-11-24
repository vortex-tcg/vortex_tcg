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
        {
            var result = await _service.GetAllAsync();
            return toActionResult<CollectionDTO[]>(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return toActionResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CollectionCreateDTO dto)
        {
            var result = await _service.CreateAsync(dto);
            return toActionResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CollectionCreateDTO dto)
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
