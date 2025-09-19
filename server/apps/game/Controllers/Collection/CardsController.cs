using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Cards.DTOs;


namespace VortexTCG.Cards.Controllers
{
    /// <summary>
    /// Contrôleur pour gérer les opérations liées aux cartes.
    /// </summary>
    [ApiController]
    [Route("api/game/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly VortexDbContext _context;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="CardsController"/>.
        /// </summary>
        /// <param name="context">Le contexte de base de données pour accéder aux données des cartes.</param>
        public CardsController(VortexDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère toutes les cartes de la base de données.
        /// </summary>
        /// <returns>Une liste de toutes les cartes disponibles.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CardDTO>>> getCards()
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

            if (cards.Count == 0)
            {
                return Ok(new { message = "Aucune carte trouvée", cards = new List<CardDTO>() });
            }

            return Ok(cards);
        }

        /// <summary>
        /// Récupère une carte spécifique par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de la carte à récupérer.</param>
        /// <returns>La carte correspondante ou une erreur 404 si elle n'existe pas.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CardDTO>> getCard(int id)
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

        /// <summary>
        /// Crée une nouvelle carte dans la base de données.
        /// </summary>
        /// <param name="createCardDTO">Les données de la carte à créer.</param>
        /// <returns>La carte créée avec ses détails ou une erreur 400 en cas de données invalides.</returns>
        [HttpPost]
        public async Task<ActionResult<CardDTO>> createCard([FromBody] CreateCardDTO create_card_dto)
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

            if (card_with_relations == null)
            {
                return StatusCode(500, new { message = "Unable to retrieve the created card" });
            }

            var card_dto = new CardDTO
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

            return CreatedAtAction(nameof(getCard), new { id = card.Id }, card_dto);
        }

        /// <summary>
        /// Met à jour une carte existante dans la base de données.
        /// </summary>
        /// <param name="id">L'identifiant de la carte à mettre à jour.</param>
        /// <param name="updateCardDTO">Les nouvelles données de la carte.</param>
        /// <returns>Une réponse 204 No Content si la mise à jour est réussie ou une erreur 404 si la carte n'existe pas.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> updateCard(int id, [FromBody] UpdateCardDTO update_card_dto)
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

        /// <summary>
        /// Supprime une carte de la base de données.
        /// </summary>
        /// <param name="id">L'identifiant de la carte à supprimer.</param>
        /// <returns>Une réponse 204 No Content si la suppression est réussie ou une erreur 404 si la carte n'existe pas.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteCard(int id)
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

        /// <summary>
        /// Recherche des cartes en fonction de filtres optionnels.
        /// </summary>
        /// <param name="name">Le nom ou une partie du nom de la carte à rechercher.</param>
        /// <param name="cardTypeId">L'identifiant du type de carte pour filtrer.</param>
        /// <param name="rarityId">L'identifiant de la rareté pour filtrer.</param>
        /// <returns>Une liste de cartes correspondant aux critères de recherche.</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CardDTO>>> searchCards([FromQuery] string? name, [FromQuery] int? card_type_id, [FromQuery] int? rarity_id)
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