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
        => toActionResult<CardDto[]>(await _service.GetAllAsync());

        [HttpPost]

        public async Task<IActionResult> Create([FromBody] CardCreateDto dto)
        => toActionResult(await _service.CreateAsync(dto));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        => toActionResult(await _service.GetByIdAsync(id));
    }
}

