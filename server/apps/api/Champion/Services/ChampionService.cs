using VortexTCG.Api.Champion.DTOs;
using VortexTCG.Api.Champion.Providers;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess.Models;
using ChampionModel = VortexTCG.DataAccess.Models.Champion;

namespace VortexTCG.Api.Champion.Services
{
	/// <summary>
	/// Service Champion - Gère la logique métier pour les opérations sur les champions.
	/// Coordonne entre le contrôleur et le fournisseur d'accès aux données.
	/// </summary>
	public class ChampionService
	{
		private readonly ChampionProvider _provider;

		/// <summary>
		/// Initialise une nouvelle instance du service Champion.
		/// </summary>
		/// <param name="provider">Le fournisseur d'accès aux données pour les champions.</param>
		public ChampionService(ChampionProvider provider)
		{
			_provider = provider;
		}

		/// <summary>
		/// Récupère tous les champions existants.
		/// Retourne une liste complète avec le nombre de champions trouvés.
		/// </summary>
		/// <returns>Un résultat contenant le tableau des DTOs de champions.</returns>
		public async Task<ResultDTO<ChampionDTO[]>> GetAllAsync()
		{
			List<ChampionModel> champions = await Task.Run(() => _provider.Query().ToList());
			ChampionDTO[] dtos = champions.Select(c => ToDTO(c)).ToArray();
			return new ResultDTO<ChampionDTO[]>
			{
				success = true,
				statusCode = 200,
				message = $"Found {dtos.Length} champion{(dtos.Length != 1 ? "s" : "")}",
				data = dtos
			};
		}

		/// <summary>
		/// Récupère un champion par son identifiant unique.
		/// Retourne le champion ou une erreur 404 s'il n'existe pas.
		/// </summary>
		/// <param name="id">L'identifiant unique du champion.</param>
		/// <returns>Un résultat contenant le DTO du champion ou une erreur.</returns>
		public async Task<ResultDTO<ChampionDTO>> GetByIdAsync(Guid id)
		{
			ChampionModel? champion = await _provider.GetByIdAsync(id);
			if (champion == null)
			{
				return new ResultDTO<ChampionDTO>
				{
					success = false,
					statusCode = 404,
					message = $"Champion with ID '{id}' not found",
					data = null
				};
			}
			return new ResultDTO<ChampionDTO>
			{
				success = true,
				statusCode = 200,
				message = $"Champion '{champion.Name}' retrieved successfully",
				data = ToDTO(champion)
			};
		}

		/// <summary>
		/// Crée un nouveau champion avec validation des données.
		/// Vérifie l'absence de champion avec le même nom.
		/// </summary>
		/// <param name="dto">L'objet de transfert de données pour la création.</param>
		/// <returns>Un résultat contenant le nouveau champion ou les erreurs de validation.</returns>
		public async Task<ResultDTO<ChampionDTO>> CreateAsync(ChampionCreateDTO dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Name))
			{
				return new ResultDTO<ChampionDTO>
				{
					success = false,
					statusCode = 400,
					message = "Invalid request: Champion name is required and cannot be empty",
					data = null
				};
			}

			ChampionModel? existingChampion = await _provider.GetByNameAsync(dto.Name);
			if (existingChampion != null)
			{
				return new ResultDTO<ChampionDTO>
				{
					success = false,
					statusCode = 409,
					message = $"Conflict: A champion named '{dto.Name}' already exists. Please use a different name.",
					data = null
				};
			}

			// Validate foreign keys: FactionId and EffectId
			List<string> validationErrors = new();
			bool hasFactionId = dto.FactionId != Guid.Empty;
			bool hasEffectId = dto.EffectId != Guid.Empty;

			if (!hasFactionId)
			{
				validationErrors.Add("Invalid request: FactionId is required.");
			}

			if (!hasEffectId)
			{
				validationErrors.Add("Invalid request: EffectId is required.");
			}

			if (validationErrors.Count == 0)
			{
				if (!await _provider.FactionExistsAsync(dto.FactionId))
				{
					validationErrors.Add($"Invalid FactionId: '{dto.FactionId}' not found.");
				}

				if (!await _provider.EffectExistsAsync(dto.EffectId))
				{
					validationErrors.Add($"Invalid EffectId: '{dto.EffectId}' not found.");
				}
			}

			if (validationErrors.Count > 0)
			{
				return new ResultDTO<ChampionDTO>
				{
					success = false,
					statusCode = 400,
					message = string.Join(" ", validationErrors),
					data = null
				};
			}

			ChampionModel champion = new ChampionModel
			{
				Id = Guid.NewGuid(),
				Name = dto.Name,
				Description = dto.Description,
				HP = dto.HP,
				Picture = dto.Picture,
                FactionId = dto.FactionId,
				EffectId = dto.EffectId
			};

			await _provider.AddAsync(champion);
			return new ResultDTO<ChampionDTO>
			{
				success = true,
				statusCode = 201,
				message = $"Champion '{champion.Name}' created successfully (ID: {champion.Id})",
				data = ToDTO(champion)
			};
		}

		/// <summary>
		/// Met à jour un champion existant avec les nouvelles données.
		/// Vérifie que le champion existe avant la mise à jour.
		/// </summary>
		/// <param name="id">L'identifiant unique du champion à mettre à jour.</param>
		/// <param name="dto">L'objet de transfert de données avec les nouvelles informations.</param>
		/// <returns>Un résultat contenant le champion mis à jour ou une erreur.</returns>
		public async Task<ResultDTO<ChampionDTO>> UpdateAsync(Guid id, ChampionCreateDTO dto)
		{
			ChampionModel? champion = await _provider.GetByIdAsync(id);
			if (champion == null)
			{
				return new ResultDTO<ChampionDTO>
				{
					success = false,
					statusCode = 404,
					message = $"Update failed: Champion with ID '{id}' not found",
					data = null
				};
			}

			if (string.IsNullOrWhiteSpace(dto.Name))
			{
				return new ResultDTO<ChampionDTO>
				{
					success = false,
					statusCode = 400,
					message = "Invalid request: Champion name is required and cannot be empty",
					data = null
				};
			}

			champion.Name = dto.Name;
			champion.Description = dto.Description;
			champion.HP = dto.HP;
			champion.Picture = dto.Picture;
			champion.FactionId = dto.FactionId;
			champion.EffectId = dto.EffectId;

			await _provider.UpdateAsync(champion);
			return new ResultDTO<ChampionDTO>
			{
				success = true,
				statusCode = 200,
				message = $"Champion '{champion.Name}' updated successfully",
				data = ToDTO(champion)
			};
		}

		/// <summary>
		/// Supprime un champion par son identifiant unique.
		/// Supprime définitivement le champion et toutes ses données associées.
		/// </summary>
		/// <param name="id">L'identifiant unique du champion à supprimer.</param>
		/// <returns>Un résultat indiquant le succès ou une erreur 404.</returns>
		public async Task<ResultDTO<object>> DeleteAsync(Guid id)
		{
			ChampionModel? champion = await _provider.GetByIdAsync(id);
			if (champion == null)
			{
				return new ResultDTO<object>
				{
					success = false,
					statusCode = 404,
					message = $"Delete failed: Champion with ID '{id}' not found",
					data = null
				};
			}

			string championName = champion.Name;
			await _provider.DeleteAsync(champion);
			return new ResultDTO<object>
			{
				success = true,
				statusCode = 200,
				message = $"Champion '{championName}' deleted successfully",
				data = null
			};
		}

		private static ChampionDTO ToDTO(ChampionModel champion)
		{
			return new ChampionDTO
			{
				Id = champion.Id,
				Name = champion.Name,
				Description = champion.Description,
				HP = champion.HP,
				Picture = champion.Picture,
				FactionId = champion.FactionId,
				EffectId = champion.EffectId
			};
		}
	}
}
