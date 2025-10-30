using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VortexTCG.Api.Logs.GameLog.DTOs;
using VortexTCG.Api.Logs.GameLog.Providers;
using GameLogModel = VortexTCG.DataAccess.Models.Gamelog;
using VortexTCG.Common.DTO;

namespace VortexTCG.Api.Logs.GameLog.Services
{
	public class GameLogService
	{
		private readonly GameLogProvider _provider;
		public GameLogService(GameLogProvider provider)
		{
			_provider = provider;
		}

		public List<GameLogDTO> GetAllAsync()
		{
			List<GameLogModel> logs = _provider.Query().ToList();
			return logs.Select(g => ToDTO(g)).ToList();
		}

		public async Task<GameLogDTO?> GetByIdAsync(Guid id)
		{
			GameLogModel? log = await _provider.GetByIdAsync(id);
			return log == null ? null : ToDTO(log);
		}

		public async Task<ResultDTO<GameLogDTO>> CreateAsync(GameLogCreateDTO dto)
		{
			GameLogModel gamelog = new GameLogModel
			{
				Id = Guid.NewGuid(),
				Label = dto.Label,
				TurnNumber = dto.TurnNumber,
				// User and Actions can be set if needed
			};
			await _provider.AddAsync(gamelog);
			return new ResultDTO<GameLogDTO>
			{
				success = true,
				statusCode = 201,
				data = ToDTO(gamelog)
			};
		}

		public async Task<ResultDTO<GameLogDTO>> UpdateAsync(Guid id, GameLogCreateDTO dto)
		{
			GameLogModel? gamelog = await _provider.GetByIdAsync(id);
			if (gamelog == null)
				return new ResultDTO<GameLogDTO> { success = false, statusCode = 404, message = "GameLog not found", data = null };
			gamelog.Label = dto.Label;
			gamelog.TurnNumber = dto.TurnNumber;
			await _provider.UpdateAsync(gamelog);
			return new ResultDTO<GameLogDTO>
			{
				success = true,
				statusCode = 200,
				data = ToDTO(gamelog)
			};
		}

		public async Task<ResultDTO<object>> DeleteAsync(Guid id)
		{
			GameLogModel? gamelog = await _provider.GetByIdAsync(id);
			if (gamelog == null)
				return new ResultDTO<object> { success = false, statusCode = 404, message = "GameLog not found", data = null };
			await _provider.DeleteAsync(gamelog);
			return new ResultDTO<object> { success = true, statusCode = 200, data = null };
		}

		private GameLogDTO ToDTO(GameLogModel gamelog)
		{
			return new GameLogDTO
			{
				Id = gamelog.Id,
				Label = gamelog.Label,
				TurnNumber = gamelog.TurnNumber,
				UserId = gamelog.User?.Id,
				ActionIds = gamelog.Actions?.Select(a => a.Id).ToList()
			};
		}
	}
}
