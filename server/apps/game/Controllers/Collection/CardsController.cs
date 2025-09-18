using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Collection.DTOs;


namespace CollectionCards.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerCards : ControllerBase
    {
        private readonly VortexDbContext _context;

        public ControllerCards(VortexDbContext context)
        {
            _context = context;
        }

        // GET: api/Cards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CardDTO>>> get_cards()
        {
            var cards = await _context.Cards
                .Include(c => c.CardType)
                .Include(c => c.Rarity)
                .Include(c => c.Extension)
                .Select(c => new CardDTO
                {
                    id = c.Id,
                    name = c.Name,
                    hp = c.Hp,
                    attack = c.Attack,
                    cost = c.Cost,
                    description = c.Description,
                    picture = c.Picture,
                    effect_active = c.Effect_active,
                    card_type_id = c.CardTypeId,
                    rarity_id = c.RarityId,
                    extension_id = c.ExtensionId,
                    // created_at = c.CreatedAt
                })
                .ToListAsync();

            return Ok(cards);
        }

        // GET: api/Cards/id
        [HttpGet("{id}")]
        public async Task<ActionResult<CardDTO>> get_card(int id)
        {
            var card = await _context.Cards
                .Include(c => c.CardType)
                .Include(c => c.Rarity)
                .Include(c => c.Extension)
                .Where(c => c.Id == id)
                .Select(c => new CardDTO
                {
                    id = c.Id,
                    name = c.Name,
                    hp = c.Hp,
                    attack = c.Attack,
                    cost = c.Cost,
                    description = c.Description,
                    picture = c.Picture,
                    effect_active = c.Effect_active,
                    card_type_id = c.CardTypeId,
                    rarity_id = c.RarityId,
                    extension_id = c.ExtensionId,
                    // created_at = c.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (card == null)
            {
                return NotFound(new { message = "Carte introuvable" });
            }

            return Ok(card);
        }

        // POST: api/Cards
        [HttpPost]
        public async Task<ActionResult<CardDTO>> create_card([FromBody] CreateCardDTO create_card_dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier que les entités liées existent
            var card_type = await _context.CardTypes.FindAsync(create_card_dto.card_type_id);
            var rarity = await _context.Rarities.FindAsync(create_card_dto.rarity_id);
            var extension = await _context.Extensions.FindAsync(create_card_dto.extension_id);

            if (card_type == null || rarity == null || extension == null)
            {
                return BadRequest(new { message = "CardType, Rarity ou Extension invalide" });
            }

            var card = new Card
            {
                Name = create_card_dto.name,
                Hp = create_card_dto.hp,
                Attack = create_card_dto.attack,
                Cost = create_card_dto.cost,
                Description = create_card_dto.description,
                Picture = create_card_dto.picture,
                Effect_active = create_card_dto.effect_active,
                CardTypeId = create_card_dto.card_type_id,
                RarityId = create_card_dto.rarity_id,
                ExtensionId = create_card_dto.extension_id,
                CreatedBy = "System" // À remplacer par l'utilisateur connecté
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            // Recharger avec les relations en une seule requête
            var card_with_relations = await _context.Cards
                .Include(c => c.CardType)
                .Include(c => c.Rarity)
                .Include(c => c.Extension)
                .FirstOrDefaultAsync(c => c.Id == card.Id);

            var cardDto = new CardDTO
            {
                id = card_with_relations.Id,
                name = card_with_relations.Name,
                hp = card_with_relations.Hp,
                attack = card_with_relations.Attack,
                cost = card_with_relations.Cost,
                description = card_with_relations.Description,
                picture = card_with_relations.Picture,
                effect_active = card_with_relations.Effect_active,
                card_type_id = card_with_relations.CardTypeId,
                rarity_id = card_with_relations.RarityId,
                extension_id = card_with_relations.ExtensionId,
                // created_at = card_with_relations.CreatedAt
            };

            return CreatedAtAction(nameof(get_card), new { id = card.Id }, cardDto);
        }

        // PUT: api/Cards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> update_card(int id, [FromBody] UpdateCardDTO update_card_dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var card = await _context.Cards.FindAsync(id);
            if (card == null)
            {
                return NotFound(new { message = "Carte introuvable" });
            }

            // Vérifier que les entités liées existent
            var card_type = await _context.CardTypes.FindAsync(update_card_dto.card_type_id);
            var rarity = await _context.Rarities.FindAsync(update_card_dto.rarity_id);
            var extension = await _context.Extensions.FindAsync(update_card_dto.extension_id);

            if (card_type == null || rarity == null || extension == null)
            {
                return BadRequest(new { message = "CardType, Rarity ou Extension invalide" });
            }

            card.Name = update_card_dto.name;
            card.Hp = update_card_dto.hp;
            card.Attack = update_card_dto.attack;
            card.Cost = update_card_dto.cost;
            card.Description = update_card_dto.description;
            card.Picture = update_card_dto.picture;
            card.Effect_active = update_card_dto.effect_active;
            card.CardTypeId = update_card_dto.card_type_id;
            card.RarityId = update_card_dto.rarity_id;
            card.ExtensionId = update_card_dto.extension_id;
            card.UpdatedBy = "System"; // À remplacer par l'utilisateur connecté

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Cards/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> delete_card(int id)
        {
            var card = await _context.Cards.FindAsync(id);
            if (card == null)
            {
                return NotFound(new { message = "Carte introuvable" });
            }

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Cards/search?name=dragon
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CardDTO>>> search_cards([FromQuery] string? name, [FromQuery] int? card_type_id, [FromQuery] int? rarity_id)
        {
            var query = _context.Cards
                .Include(c => c.CardType)
                .Include(c => c.Rarity)
                .Include(c => c.Extension)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.Name.Contains(name) && c.Name != null);
            }

            if (card_type_id.HasValue)
            {
                query = query.Where(c => c.CardTypeId == card_type_id.Value);
            }

            if (rarity_id.HasValue)
            {
                query = query.Where(c => c.RarityId == rarity_id.Value);
            }


            var cards = await query
                .Select(c => new CardDTO
                {
                    id = c.Id,
                    name = c.Name,
                    hp = c.Hp,
                    attack = c.Attack,
                    cost = c.Cost,
                    description = c.Description,
                    picture = c.Picture,
                    effect_active = c.Effect_active,
                    card_type_id = c.CardTypeId,
                    rarity_id = c.RarityId,
                    extension_id = c.ExtensionId,
                    // created_at = c.CreatedAt
                })
                .ToListAsync();

            return Ok(cards);
        }
    }
}