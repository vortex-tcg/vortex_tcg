using Microsoft.EntityFrameworkCore;
using VortexTCG.Faction.DTOs;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.Faction.Providers {

    public class FactionProvider
    {
        private readonly VortexDbContext _db;

        public FactionProvider(VortexDbContext db)
        {
            _db = db;
        }

        public async Task<(bool IsValid, List<Guid> InvalidIds)> ValidateCardIds(List<Guid> cardIds)
        {
            if (cardIds == null || !cardIds.Any())
                return (true, new List<Guid>());

            var existingCardIds = await _db.Cards
                .Where(c => cardIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            var invalidIds = cardIds.Except(existingCardIds).ToList();
            return (invalidIds.Count == 0, invalidIds);
        }

        public async Task<bool> ValidateChampionId(Guid championId)
        {
            if (championId == Guid.Empty)
                return true;

            return await _db.Champions.AnyAsync(ch => ch.Id == championId);
        }

        public async Task<List<FactionDto>> GetAllFactions()
        {
            return await _db.Factions
                            .Select(f => new FactionDto
                            {
                                Id = f.Id,
                                Label = f.Label,
                                Currency = f.Currency,
                                Condition = f.Condition
                            })
                            .ToListAsync();
        }

        public async Task<FactionDto?> GetFactionById(Guid id)
        {
            return await _db.Factions
                            .Where(f => f.Id == id)
                            .Select(f => new FactionDto
                            {
                                Id = f.Id,
                                Label = f.Label,
                                Currency = f.Currency,
                                Condition = f.Condition
                            })
                            .SingleOrDefaultAsync();
        }

        public async Task<FactionWithCardsDto?> GetFactionWithCardsById(Guid id)
        {
            return await _db.Factions
                            .Where(f => f.Id == id)
                            .Select(f => new FactionWithCardsDto
                            {
                                Id = f.Id,
                                Label = f.Label,
                                Currency = f.Currency,
                                Condition = f.Condition,
                                Cards = f.Cards.Select(fc => new FactionCardDto
                                {
                                    Id = fc.Card.Id,
                                    Name = fc.Card.Name,
                                    Price = fc.Card.Price,
                                    Hp = fc.Card.Hp,
                                    Attack = fc.Card.Attack,
                                    Cost = fc.Card.Cost,
                                    Description = fc.Card.Description,
                                    Picture = fc.Card.Picture,
                                    Extension = fc.Card.Extension.ToString(),
                                    CardType = fc.Card.CardType.ToString()
                                }).ToList()
                            })
                            .SingleOrDefaultAsync();
        }

        public async Task<FactionWithChampionDto?> GetFactionWithChampionById(Guid id)    
        {
            return await _db.Factions
                            .Where(f => f.Id == id)
                            .Select(f => new FactionWithChampionDto
                            {
                                Id = f.Id,
                                Label = f.Label,
                                Currency = f.Currency,
                                Condition = f.Condition,
                                Champion = f.Champions.Select(champion => new FactionChampionDto
                                {
                                    Id = champion.Id,
                                    Name = champion.Name,
                                    Description = champion.Description,
                                    HP = champion.HP,
                                    Picture = champion.Picture,
                                }).FirstOrDefault()
                            })
                            .SingleOrDefaultAsync();
        }

        public async Task<(bool Success, FactionDto? Result, string ErrorMessage)> CreateFaction(CreateFactionDto createDto)
        {
            var (cardValidation, invalidCardIds) = await ValidateCardIds(createDto.CardIds);

            if (!cardValidation)
            {
                return (false, null, $"Les IDs de cartes suivants n'existent pas : {string.Join(", ", invalidCardIds)}");
            }

            if (createDto.ChampionId.HasValue)
            {
                var championValid = await ValidateChampionId(createDto.ChampionId.Value);
                if (!championValid)
                {
                    return (false, null, $"L'ID du champion {createDto.ChampionId.Value} n'existe pas");
                }
            }

            var faction = new VortexTCG.DataAccess.Models.Faction
            {
                Id = Guid.NewGuid(),
                Label = createDto.Label,
                Currency = createDto.Currency,
                Condition = createDto.Condition,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedAtUtc = DateTime.UtcNow,
                UpdatedBy = "System"
            };

            _db.Factions.Add(faction);


            if (createDto.CardIds.Any())
            {
                var existingCards = await _db.Cards
                    .Where(c => createDto.CardIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var card in existingCards)
                {
                    var factionCard = new VortexTCG.DataAccess.Models.FactionCard
                    {
                        Id = Guid.NewGuid(),
                        FactionId = faction.Id,
                        CardId = card.Id,
                        CreatedAtUtc = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedAtUtc = DateTime.UtcNow,
                        UpdatedBy = "System"
                    };
                    _db.FactionCards.Add(factionCard);
                }
            }


            if (createDto.ChampionId.HasValue)
            {
                var existingChampion = await _db.Champions
                    .FirstOrDefaultAsync(c => c.Id == createDto.ChampionId.Value);

                if (existingChampion != null)
                {

                    existingChampion.FactionId = faction.Id;
                    existingChampion.UpdatedAtUtc = DateTime.UtcNow;
                    existingChampion.UpdatedBy = "System";
                }
            }

            await _db.SaveChangesAsync();

            var result = new FactionDto
            {
                Id = faction.Id,
                Label = faction.Label,
                Currency = faction.Currency,
                Condition = faction.Condition
            };

            return (true, result, string.Empty);
        }

        public async Task<(bool Success, FactionDto? Result, string ErrorMessage)> UpdateFaction(Guid id, UpdateFactionDto updateDto)
        {
            var faction = await _db.Factions.FindAsync(id);
            if (faction == null)
                return (false, null, "Faction non trouvée");

            if (updateDto.CardIds != null)
            {
                var (cardValidation, invalidCardIds) = await ValidateCardIds(updateDto.CardIds);
                if (!cardValidation)
                {
                    return (false, null, $"Les IDs de cartes suivants n'existent pas : {string.Join(", ", invalidCardIds)}");
                }
            }

            if (updateDto.ChampionId.HasValue)
            {
                var championValid = await ValidateChampionId(updateDto.ChampionId.Value);
                if (!championValid)
                {
                    return (false, null, $"L'ID du champion {updateDto.ChampionId.Value} n'existe pas");
                }
            }

            if (!string.IsNullOrEmpty(updateDto.Label))
                faction.Label = updateDto.Label;
            
            if (!string.IsNullOrEmpty(updateDto.Currency))
                faction.Currency = updateDto.Currency;
            
            if (updateDto.Condition != null)
                faction.Condition = updateDto.Condition;

            if (updateDto.CardIds != null)
            {
                var existingFactionCards = await _db.FactionCards
                    .Where(fc => fc.FactionId == id)
                    .ToListAsync();
                _db.FactionCards.RemoveRange(existingFactionCards);

                foreach (var cardId in updateDto.CardIds)
                {
                    var factionCard = new VortexTCG.DataAccess.Models.FactionCard
                    {
                        Id = Guid.NewGuid(),
                        FactionId = faction.Id,
                        CardId = cardId,
                        CreatedAtUtc = DateTime.UtcNow,
                        CreatedBy = "System",
                        UpdatedAtUtc = DateTime.UtcNow,
                        UpdatedBy = "System"
                    };
                    _db.FactionCards.Add(factionCard);
                }
            }

            if (updateDto.ChampionId.HasValue)
            {
                var oldChampion = await _db.Champions
                    .FirstOrDefaultAsync(c => c.FactionId == id);
                if (oldChampion != null)
                {
                    oldChampion.FactionId = Guid.Empty;
                    oldChampion.UpdatedAtUtc = DateTime.UtcNow;
                    oldChampion.UpdatedBy = "System";
                }

                if (updateDto.ChampionId.Value != Guid.Empty)
                {
                    var newChampion = await _db.Champions
                        .FirstOrDefaultAsync(c => c.Id == updateDto.ChampionId.Value);
                    if (newChampion != null)
                    {
                        newChampion.FactionId = faction.Id;
                        newChampion.UpdatedAtUtc = DateTime.UtcNow;
                        newChampion.UpdatedBy = "System";
                    }
                }
            }

            faction.UpdatedAtUtc = DateTime.UtcNow;
            faction.UpdatedBy = "System";

            await _db.SaveChangesAsync();

            var result = new FactionDto
            {
                Id = faction.Id,
                Label = faction.Label,
                Currency = faction.Currency,
                Condition = faction.Condition
            };

            return (true, result, string.Empty);
        }

        public async Task<bool> DeleteFaction(Guid id)
        {
            var faction = await _db.Factions.FindAsync(id);
            if (faction == null)
                return false;

            _db.Factions.Remove(faction);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FactionExists(Guid id)
        {
            return await _db.Factions.AnyAsync(f => f.Id == id);
        }


        public async Task<bool> LabelExists(string label, Guid? excludeId = null)
        {
            return await _db.Factions
                           .Where(f => f.Label == label && (excludeId == null || f.Id != excludeId))
                           .AnyAsync();
        }
    }
}