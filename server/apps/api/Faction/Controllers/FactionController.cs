using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;
using VortexTCG.Common.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Faction.Services;
using VortexTCG.Faction.DTOs;

namespace VortexTCG.Faction.Controllers
{

    [ApiController]
    [Route("api/faction")]
    public class FactionController : VortexBaseController
    {
        private readonly VortexDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly FactionService _faction_service;

        public FactionController(VortexDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _faction_service = new FactionService(_db, _configuration);
        }

        // ================= Get all factions =================
        [HttpGet]
        public async Task<IActionResult> GetAllFactions()
        => toActionResult(await _faction_service.GetAllFactions());

        // ================= Get faction by ID =================
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFactionById(Guid id)
        => toActionResult(await _faction_service.GetFactionById(id));

        // ================= Get faction with cards by ID =================
        [HttpGet("{id:guid}/cards")]
        public async Task<IActionResult> GetFactionWithCardsById(Guid id)
        => toActionResult(await _faction_service.GetFactionWithCardsById(id));

        // ================= Get faction with champions by ID =================
        [HttpGet("{id:guid}/champions")]
        public async Task<IActionResult> GetFactionWithChampionsById(Guid id)
        => toActionResult(await _faction_service.GetFactionWithChampionById(id));

        // ================= Create faction =================
        [HttpPost]
        public async Task<IActionResult> CreateFaction([FromBody] CreateFactionDto createDto)
        => toActionResult(await _faction_service.CreateFaction(createDto));

        // ================= Update faction =================
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateFaction(Guid id, [FromBody] UpdateFactionDto updateDto)
        => toActionResult(await _faction_service.UpdateFaction(id, updateDto));

        // ================= Delete faction =================
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteFaction(Guid id)
        => toActionResult(await _faction_service.DeleteFaction(id));
    }
}