using Microsoft.AspNetCore.Mvc;
using Deck.DTOs;
using Deck.Services;

namespace Deck.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeckController : ControllerBase
    {
        private readonly DeckService _deckService;

        public DeckController()
        {
            _deckService = new DeckService();
        }

        [HttpGet("{deckId}")]
        public ActionResult<DeckDTO> GetDeckById(string deckId)
        {
            DeckDTO deck = _deckService.GetDeckById(deckId);
            return Ok(deck);
        }
    }
}
