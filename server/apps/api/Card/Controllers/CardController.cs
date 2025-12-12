using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Card.DTOs;
using VortexTCG.Api.Card.Services;
using VortexTCG.Common.Services;
using VortexTCG.Common.DTO;

namespace VortexTCG.Api.Card.Controllers
{
    [ApiController]
    [Route("api/card")]

    public class CardController : VortexBaseController
    {
        private readonly CardService _service;
        public CardController(CardService service) => _service = service;

		[HttpGet]

		public async Task<IActionResult> GetAll() 
        {
		    var result = await _service.GetAllAsync();
            return toActionResult<CardDTO[]>(result);
        }

        [HttpPost]

        public async Task<IActionResult> Create([FromBody] CardCreateDTO dto)
        {
            var result = await _service.CreateAsync(dto);
            return toActionResult(result);   
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return toActionResult(result);
        }
    }
}

