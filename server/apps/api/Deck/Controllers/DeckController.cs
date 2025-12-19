using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Services;
using VortexTCG.Common.Services;

namespace VortexTCG.Api.Deck.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeckController : VortexBaseController
    {
        private readonly DeckService _deckService;

        public DeckController()
        {
            _deckService = new DeckService();
        }

        [HttpGet("{deckId}")]
        public async Task<IActionResult> GetDeckById(string deckId)
        => toActionResult(_deckService.GetDeckById(deckId));
    }
}
