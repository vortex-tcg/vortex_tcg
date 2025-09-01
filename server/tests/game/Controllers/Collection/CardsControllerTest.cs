using Xunit;
using CollectionCards.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Collection.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Collection
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly VortexDbContext _context;

        public CardsController(VortexDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CardDTO>>> GetAllCards()
        {
            var cards = await _context.Cards
                .Include(c => c.CardType)
                .Include(c => c.Rarity)
                .Include(c => c.Extension)
                .Select(c => new CardDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Hp = c.Hp,
                    Attack = c.Attack,
                    Cost = c.Cost,
                    Description = c.Description,
                    Picture = c.Picture,
                    Effect_active = c.Effect_active,
                    CardTypeId = c.CardTypeId,
                    RarityId = c.RarityId,
                    ExtensionId = c.ExtensionId,
                })
                .ToListAsync();

            return Ok(cards);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CardDTO>> GetCardById(int id)
        {
            var card = await _context.Cards
                .Include(c => c.CardType)
                .Include(c => c.Rarity)
                .Include(c => c.Extension)
                .Where(c => c.Id == id)
                .Select(c => new CardDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Hp = c.Hp,
                    Attack = c.Attack,
                    Cost = c.Cost,
                    Description = c.Description,
                    Picture = c.Picture,
                    Effect_active = c.Effect_active,
                    CardTypeId = c.CardTypeId,
                    RarityId = c.RarityId,
                    ExtensionId = c.ExtensionId,
                })
                .FirstOrDefaultAsync();

            if (card == null)
            {
                return NotFound();
            }

            return Ok(card);
        }
    }
}

