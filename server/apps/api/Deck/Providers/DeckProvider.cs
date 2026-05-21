using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.DataAccess;
using DeckModel = VortexTCG.DataAccess.Models.Deck;
using DeckCardModel = VortexTCG.DataAccess.Models.DeckCard;
using CollectionCardModel = VortexTCG.DataAccess.Models.CollectionCard;
using ClassCardModel = VortexTCG.DataAccess.Models.ClassCard;

namespace VortexTCG.Api.Deck.Providers
{
    public class DeckProvider
    {
        private readonly VortexDbContext _db;

        public DeckProvider(VortexDbContext db)
        {
            _db = db;
        }

        public async Task<DeckModel?> GetByIdAsync(Guid deckId)
            => await _db.Decks.AsNoTracking().FirstOrDefaultAsync(d => d.Id == deckId);

        public async Task<DeckModel> AddAsync(DeckModel deck)
        {
            await _db.Decks.AddAsync(deck);
            await _db.SaveChangesAsync();
            return deck;
        }

        public async Task<bool> DeleteAsync(Guid deckId)
        {
            DeckModel? deck = await _db.Decks.FindAsync(deckId);
            if (deck == null) return false;

            _db.Decks.Remove(deck);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<DeckModel?> GetDeckWithCardsAndChampionAsync(Guid deckId)
        {
            return await _db.Decks
                .AsNoTracking()
                .Include(d => d.DeckCard)
                .ThenInclude((DeckCardModel dc) => dc.Card)
                .ThenInclude((CollectionCardModel cc) => cc.Card)
                .Include(d => d.Champion)
                .FirstOrDefaultAsync(d => d.Id == deckId);
        }

        public async Task<List<(Guid CardId, string Label)>> GetClassRowsByCardIdsAsync(List<Guid> cardIds)
        {
            return await _db.Set<ClassCardModel>()
                .AsNoTracking()
                .Include(x => x.Class)
                .Where(x => cardIds.Contains(x.CardId))
                .Select(x => new ValueTuple<Guid, string>(
                    x.CardId,
                    x.Class != null ? x.Class.Label : ""
                ))
                .ToListAsync();
        }
        public async Task<DeckModel?> GetDeckForUpdateAsync(Guid deckId)
        {
            return await _db.Decks
                .Include(d => d.DeckCard)
                .FirstOrDefaultAsync(d => d.Id == deckId);
        }

        public async Task UpdateDeckAsync(DeckModel deck, List<UpdateDeckCardDto> newCards)
        {

            await _db.DeckCards
                .Where(dc => dc.DeckId == deck.Id)
                .ExecuteDeleteAsync();

            var newDeckCards = newCards.Select(card => new DeckCardModel
            {
                Id = Guid.NewGuid(),
                DeckId = deck.Id,
                CardId = card.CollectionCardId,
                Quantity = card.Quantity,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedAtUtc = null,
                UpdatedBy = null
            }).ToList();

            await _db.DeckCards.AddRangeAsync(newDeckCards);
            await _db.SaveChangesAsync();
        }
        public virtual async Task<List<DeckModel>> GetDecksByUserIdAsync(Guid userId)
        {
            return await _db.Decks
                .AsNoTracking()
                .Include(d => d.Champion)
                .Include(d => d.Faction)
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }
    }
}