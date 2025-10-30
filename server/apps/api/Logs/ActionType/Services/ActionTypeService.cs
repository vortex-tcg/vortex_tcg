
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VortexTCG.Api.Logs.ActionType.DTOs;
using VortexTCG.Api.Logs.ActionType.Providers;
using ActionTypeModel = VortexTCG.DataAccess.Models.ActionType;
using VortexTCG.Common.DTO;

namespace VortexTCG.Api.Logs.ActionType.Services
{
	public class ActionTypeService
	{
		private readonly ActionTypeProvider _provider;
		public ActionTypeService(ActionTypeProvider provider)
		{
			_provider = provider;
		}

		public async Task<ResultDTO<ActionTypeDTO[]>> GetAllAsync()
		{
			List<ActionTypeModel> actions = await Task.Run(() => _provider.Query().ToList());
			ActionTypeDTO[] dtos = actions.Select(a => ToDTO(a)).ToArray();
			return new ResultDTO<ActionTypeDTO[]>
			{
				success = true,
				statusCode = 200,
				data = dtos
			};
		}

		public async Task<ActionTypeDTO?> GetByIdAsync(Guid id)
		{
			ActionTypeModel? action = await _provider.GetByIdAsync(id);
			return action == null ? null : ToDTO(action);
		}

		public async Task<ResultDTO<ActionTypeDTO>> CreateAsync(ActionTypeCreateDTO dto)
		{
			ActionTypeModel actionType = new ActionTypeModel
			{
				Id = Guid.NewGuid(),
				actionDescription = dto.ActionDescription,
				GameLogId = dto.GameLogId,
				ParentId = dto.ParentId ?? Guid.Empty
			};
			await _provider.AddAsync(actionType);
			return new ResultDTO<ActionTypeDTO>
			{
				success = true,
				statusCode = 201,
				data = ToDTO(actionType)
			};
		}

		public async Task<ResultDTO<ActionTypeDTO>> UpdateAsync(Guid id, ActionTypeCreateDTO dto)
		{
			ActionTypeModel? actionType = await _provider.GetByIdAsync(id);
			if (actionType == null)
				return new ResultDTO<ActionTypeDTO> { success = false, statusCode = 404, message = "ActionType not found", data = null };
			actionType.actionDescription = dto.ActionDescription;
			actionType.GameLogId = dto.GameLogId;
			actionType.ParentId = dto.ParentId ?? Guid.Empty;
			await _provider.UpdateAsync(actionType);
			return new ResultDTO<ActionTypeDTO>
			{
				success = true,
				statusCode = 200,
				data = ToDTO(actionType)
			};
		}

		public async Task<ResultDTO<object>> DeleteAsync(Guid id)
		{
			ActionTypeModel? actionType = await _provider.GetByIdAsync(id);
			if (actionType == null)
				return new ResultDTO<object> { success = false, statusCode = 404, message = "ActionType not found", data = null };
			await _provider.DeleteAsync(actionType);
			return new ResultDTO<object> { success = true, statusCode = 200, data = null };
		}

		private ActionTypeDTO ToDTO(ActionTypeModel actionType)
		{
			return new ActionTypeDTO
			{
				Id = actionType.Id,
				ActionDescription = actionType.actionDescription,
				GameLogId = actionType.GameLogId,
				ParentId = actionType.ParentId != Guid.Empty ? actionType.ParentId : null,
				ChildIds = actionType.Childs?.Select(c => c.Id).ToList()
			};
		}
	}
}
