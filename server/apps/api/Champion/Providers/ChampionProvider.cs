using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using VortexTCG.DataAccess;
using ChampionModel = VortexTCG.DataAccess.Models.Champion;

namespace VortexTCG.Api.Champion.Providers
{
	/// <summary>
	/// Fournisseur Champion - Gère l'accès aux données des champions dans la base de données.
	/// Encapsule toutes les opérations CRUD pour le modèle Champion.
	/// </summary>
	public class ChampionProvider
	{
		private readonly VortexDbContext _db;

		/// <summary>
		/// Initialise une nouvelle instance du fournisseur Champion.
		/// </summary>
		/// <param name="db">Le contexte de la base de données.</param>
		public ChampionProvider(VortexDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Retourne une requête queryable sur la table des champions.
		/// </summary>
		/// <returns>Une requête queryable pour interroger les champions.</returns>
		public IQueryable<ChampionModel> Query() => _db.Champions.AsQueryable();

		/// <summary>
		/// Récupère un champion par son identifiant unique.
		/// </summary>
		/// <param name="id">L'identifiant du champion recherché.</param>
		/// <returns>Le champion s'il existe, sinon null.</returns>
		public async Task<ChampionModel?> GetByIdAsync(Guid id)
		{
			return await _db.Champions.FirstOrDefaultAsync(c => c.Id == id);
		}

		/// <summary>
		/// Récupère un champion par son nom.
		/// </summary>
		/// <param name="name">Le nom du champion recherché.</param>
		/// <returns>Le champion s'il existe, sinon null.</returns>
		public async Task<ChampionModel?> GetByNameAsync(string name)
		{
			return await _db.Champions.FirstOrDefaultAsync(c => c.Name == name);
		}

		/// <summary>
		/// Ajoute un nouveau champion dans la base de données.
		/// Enregistre les modifications immédiatement.
		/// </summary>
		/// <param name="champion">Le champion à ajouter.</param>
		public async Task AddAsync(ChampionModel champion)
		{
			await _db.Champions.AddAsync(champion);
			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Met à jour un champion existant dans la base de données.
		/// Enregistre les modifications immédiatement.
		/// </summary>
		/// <param name="champion">Le champion avec les modifications à appliquer.</param>
		public async Task UpdateAsync(ChampionModel champion)
		{
			_db.Champions.Update(champion);
			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Supprime un champion de la base de données.
		/// Enregistre les modifications immédiatement.
		/// </summary>
		/// <param name="champion">Le champion à supprimer.</param>
		public async Task DeleteAsync(ChampionModel champion)
		{
			_db.Champions.Remove(champion);
			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Vérifie l'existence d'une Faction par son identifiant.
		/// </summary>
		/// <param name="factionId">Identifiant de la faction.</param>
		/// <returns>true si la faction existe, sinon false.</returns>
		public Task<bool> FactionExistsAsync(Guid factionId)
		{
			return _db.Factions.AnyAsync(f => f.Id == factionId);
		}

		/// <summary>
		/// Vérifie l'existence d'un Effet par son identifiant.
		/// </summary>
		/// <param name="effectId">Identifiant de l'effet.</param>
		/// <returns>true si l'effet existe, sinon false.</returns>
		public Task<bool> EffectExistsAsync(Guid effectId)
		{
			return _db.Effects.AnyAsync(e => e.Id == effectId);
		}
	}
}