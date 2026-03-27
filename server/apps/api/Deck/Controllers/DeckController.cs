using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Interface;
using VortexTCG.Api.Deck.Services;
using VortexTCG.Common.Services;

namespace VortexTCG.Api.Deck.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeckController : VortexBaseController
    {
        private readonly IDeckService _deckService;

        public DeckController(IDeckService deckService)
        {
            _deckService = deckService;
        }

        [HttpGet("{deckId}")]
        public async Task<IActionResult> GetDeckById(string deckId)
        => toActionResult(_deckService.GetDeckById(deckId));
        

        [HttpGet("getDeckData/{deckId:guid}")]
        public async Task<IActionResult> GetDeckData(Guid deckId)
            => toActionResult(await _deckService.GetDeckDataAsync(deckId));
    }
}
