using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Champion.DTOs;
using VortexTCG.Api.Champion.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;

namespace VortexTCG.Api.Champion.Controllers
{
    /// <summary>
    /// Contrôleur Champion - Gère tous les points de terminaison API liés aux champions.
    /// Fournit les opérations CRUD pour les champions du jeu.
    /// </summary>
    [ApiController]
    [Route("api/champion")]
    public class ChampionController : VortexBaseController
    {
        private readonly ChampionService _service;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur Champion.
        /// </summary>
        /// <param name="service">Le service champion pour les opérations de logique métier.</param>
        public ChampionController(ChampionService service)
        {
            _service = service;
        }

        /// <summary>
        /// Récupère tous les champions de la base de données.
        /// Retourne la liste complète des champions disponibles.
        /// </summary>
        /// <returns>Un résultat contenant le tableau de tous les champions.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            ResultDTO<ChampionDTO[]> result = await _service.GetAllAsync();
            return toActionResult<ChampionDTO[]>(result);
        }

        /// <summary>
        /// Récupère un champion spécifique par son identifiant unique.
        /// Retourne les détails du champion s'il existe, sinon une erreur 404.
        /// </summary>
        /// <param name="id">L'identifiant unique du champion.</param>
        /// <returns>Un résultat contenant les données du champion ou une erreur.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            ResultDTO<ChampionDTO> result = await _service.GetByIdAsync(id);
            return toActionResult<ChampionDTO>(result);
        }

        /// <summary>
        /// Crée un nouveau champion avec les données fournies.
        /// Valide l'entrée et vérifie qu'il n'existe pas de champion avec le même nom.
        /// </summary>
        /// <param name="dto">L'objet de transfert de données contenant les détails de création.</param>
        /// <returns>Un résultat contenant le champion créé ou une erreur de validation.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ChampionCreateDTO dto)
        {
            ResultDTO<ChampionDTO> result = await _service.CreateAsync(dto);
            return toActionResult<ChampionDTO>(result);
        }

        /// <summary>
        /// Met à jour un champion existant avec les nouvelles données.
        /// Vérifie que le champion existe et met à jour tous les champs fournis.
        /// </summary>
        /// <param name="id">L'identifiant unique du champion à mettre à jour.</param>
        /// <param name="dto">L'objet de transfert de données contenant les détails mis à jour.</param>
        /// <returns>Un résultat contenant le champion mis à jour ou une erreur.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ChampionCreateDTO dto)
        {
            ResultDTO<ChampionDTO> result = await _service.UpdateAsync(id, dto);
            return toActionResult<ChampionDTO>(result);
        }

        /// <summary>
        /// Supprime un champion par son identifiant unique.
        /// Supprime définitivement le champion de la base de données.
        /// </summary>
        /// <param name="id">L'identifiant unique du champion à supprimer.</param>
        /// <returns>Un résultat indiquant le succès ou une erreur 404 s'il n'est pas trouvé.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            ResultDTO<object> result = await _service.DeleteAsync(id);
            return toActionResult<object>(result);
        }
    }
}
