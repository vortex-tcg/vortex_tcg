using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Deck.Interface;
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

        [HttpGet("getDeckData/{deckId:guid}")]
        public async Task<IActionResult> GetDeckData(Guid deckId)
            => toActionResult(await _deckService.GetDeckDataAsync(deckId));

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetDecksByUserId(Guid userId)
            => toActionResult(await _deckService.GetDecksByUserIdAsync(userId));
    }
}