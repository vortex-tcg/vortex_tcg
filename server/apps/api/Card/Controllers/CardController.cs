using Microsoft.AspNetCore.Mvc;
using api.Card.DTOs;
using api.Card.Services;
using VortexTCG.Common.Services;
using VortexTCG.Common.DTO;

namespace api.Card.Controllers
{
    [ApiController]
    [Route("api/card")]

    public class CardController : VortexBaseController
    {
        private readonly CardService _service;
        public CardController(CardService service) => _service = service;

		[HttpGet]

		public async Task<IActionResult> list(Guid id, CancellationToken ct) 
        {
		    return toActionResult(await _service.listAsync(ct));
        }

        [HttpPost]

        public async Task<IActionResult> create([FromBody] CardCreateDTO dto, CancellationToken ct)
        {
            return toActionResult(await _service.createAsync(dto, ct));   
        }
    }
}

