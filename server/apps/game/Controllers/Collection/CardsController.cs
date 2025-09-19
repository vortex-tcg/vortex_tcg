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
        public async Task<ActionResult<IEnumerable<CardDTO>>> GetCards()
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
                    // CreatedAt = c.CreatedAt
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
        public async Task<ActionResult<CardDTO>> GetCard(int id)
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
                    // CreatedAt = c.CreatedAt
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
        public async Task<ActionResult<CardDTO>> CreateCard([FromBody] CreateCardDTO createCardDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Vérifier que les entités liées existent
            var cardType = await _context.CardTypes.FindAsync(createCardDTO.CardTypeId);
            var rarity = await _context.Rarities.FindAsync(createCardDTO.RarityId);
            var extension = await _context.Extensions.FindAsync(createCardDTO.ExtensionId);

            if (cardType == null || rarity == null || extension == null)
            {
                return BadRequest(new { message = "CardType, Rarity ou Extension invalide" });
            }

            var card = new Card
            {
                Name = createCardDTO.Name,
                Hp = createCardDTO.Hp,
                Attack = createCardDTO.Attack,
                Cost = createCardDTO.Cost,
                Description = createCardDTO.Description,
                Picture = createCardDTO.Picture,
                Effect_active = createCardDTO.Effect_active,
                CardTypeId = createCardDTO.CardTypeId,
                RarityId = createCardDTO.RarityId,
                ExtensionId = createCardDTO.ExtensionId,
                CreatedBy = "System" // À remplacer par l'utilisateur connecté
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            // Recharger avec les relations en une seule requête
            var cardWithRelations = await _context.Cards
                .Include(c => c.CardType)
                .Include(c => c.Rarity)
                .Include(c => c.Extension)
                .FirstOrDefaultAsync(c => c.Id == card.Id);

            if (cardWithRelations == null)
            {
                return StatusCode(500, new { message = "Unable to retrieve the created card" });
            }

            var cardDto = new CardDTO
            {
                Id = cardWithRelations.Id,
                Name = cardWithRelations.Name,
                Hp = cardWithRelations.Hp,
                Attack = cardWithRelations.Attack,
                Cost = cardWithRelations.Cost,
                Description = cardWithRelations.Description,
                Picture = cardWithRelations.Picture,
                Effect_active = cardWithRelations.Effect_active,
                CardTypeId = cardWithRelations.CardTypeId,
                RarityId = cardWithRelations.RarityId,
                ExtensionId = cardWithRelations.ExtensionId,
                // CreatedAt = cardWithRelations.CreatedAt
            };

            return CreatedAtAction(nameof(GetCard), new { id = card.Id }, cardDto);
        }

        /// <summary>
        /// Met à jour une carte existante dans la base de données.
        /// </summary>
        /// <param name="id">L'identifiant de la carte à mettre à jour.</param>
        /// <param name="updateCardDTO">Les nouvelles données de la carte.</param>
        /// <returns>Une réponse 204 No Content si la mise à jour est réussie ou une erreur 404 si la carte n'existe pas.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCard(int id, [FromBody] UpdateCardDTO updateCardDTO)
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
            var cardType = await _context.CardTypes.FindAsync(updateCardDTO.CardTypeId);
            var rarity = await _context.Rarities.FindAsync(updateCardDTO.RarityId);
            var extension = await _context.Extensions.FindAsync(updateCardDTO.ExtensionId);

            if (cardType == null || rarity == null || extension == null)
            {
                return BadRequest(new { message = "CardType, Rarity ou Extension invalide" });
            }

            card.Name = updateCardDTO.Name;
            card.Hp = updateCardDTO.Hp;
            card.Attack = updateCardDTO.Attack;
            card.Cost = updateCardDTO.Cost;
            card.Description = updateCardDTO.Description;
            card.Picture = updateCardDTO.Picture;
            card.Effect_active = updateCardDTO.Effect_active;
            card.CardTypeId = updateCardDTO.CardTypeId;
            card.RarityId = updateCardDTO.RarityId;
            card.ExtensionId = updateCardDTO.ExtensionId;
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
        public async Task<IActionResult> DeleteCard(int id)
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
        public async Task<ActionResult<IEnumerable<CardDTO>>> SearchCards([FromQuery] string? name, [FromQuery] int? cardTypeId, [FromQuery] int? rarityId)
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

            if (cardTypeId.HasValue)
            {
                query = query.Where(c => c.CardTypeId == cardTypeId.Value);
            }

            if (rarityId.HasValue)
            {
                query = query.Where(c => c.RarityId == rarityId.Value);
            }

            var cards = await query
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
                    // CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(cards);
        }
    }
}