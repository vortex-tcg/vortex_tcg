using Microsoft.EntityFrameworkCore;
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

        public async Task<List<DeckModel>> GetDecksByUserIdAsync(Guid userId)
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