using VortexTCG.Api.User.DTOs;
using VortexTCG.Api.User.Providers;
using VortexTCG.DataAccess.Models;
using VortexTCG.Common.DTO;
using UserModel = VortexTCG.DataAccess.Models.User;

namespace VortexTCG.Api.User.Services
{
	public class UserService
	{
		private readonly UserProvider _provider;
		public UserService(UserProvider provider)
		{
			_provider = provider;
		}

		public async Task<List<UserDTO>> GetAllAsync()
		{
			List<UserModel> users = _provider.Query().ToList();
			List<UserDTO> dtos = users.Select(u => ToDTO(u)).ToList();
			return dtos;
		}

		public async Task<UserDTO?> GetByIdAsync(Guid id)
		{
			UserModel? user = await _provider.GetByIdAsync(id);
			return user == null ? null : ToDTO(user);
		}

		public async Task<ResultDTO<UserDTO>> CreateAsync(UserCreateDTO dto)
		{
			UserModel? existingUser = await _provider.GetByUsernameAsync(dto.Username);
			if (existingUser != null)
			{
				return new ResultDTO<UserDTO>
				{
					success = false,
					statusCode = 409,
					message = "Username already exists",
					data = null
				};
			}
			UserModel user = new UserModel
			{
				Id = Guid.NewGuid(),
				FirstName = dto.FirstName,
				LastName = dto.LastName,
				Username = dto.Username,
				Password = dto.Password,
				Email = dto.Email,
				CurrencyQuantity = dto.CurrencyQuantity,
				Language = dto.Language,
				Role = dto.Role,
				Status = dto.Status,
				RankId = dto.RankId
			};
			await _provider.AddAsync(user);
			return new ResultDTO<UserDTO>
			{
				success = true,
				statusCode = 201,
				message = null,
				data = ToDTO(user)
			};
		}

		public async Task<ResultDTO<UserDTO>> UpdateAsync(Guid id, UserCreateDTO dto)
		{
			UserModel? user = await _provider.GetByIdAsync(id);
			if (user == null)
				return new ResultDTO<UserDTO>
				{
					success = false,
					statusCode = 404,
					message = "User not found",
					data = null
				};
			user.FirstName = dto.FirstName;
			user.LastName = dto.LastName;
			user.Username = dto.Username;
			user.Password = dto.Password;
			user.Email = dto.Email;
			user.CurrencyQuantity = dto.CurrencyQuantity;
			user.Language = dto.Language;
			user.Role = dto.Role;
			user.Status = dto.Status;
			user.RankId = dto.RankId;
			await _provider.UpdateAsync(user);
			return new ResultDTO<UserDTO>
			{
				success = true,
				statusCode = 200,
				message = null,
				data = ToDTO(user)
			};
		}

		public async Task<ResultDTO<object>> DeleteAsync(Guid id)
		{
			UserModel? user = await _provider.GetByIdAsync(id);
			if (user == null)
				return new ResultDTO<object>
				{
					success = false,
					statusCode = 404,
					message = "User not found",
					data = null
				};
			await _provider.DeleteAsync(user);
			return new ResultDTO<object>
			{
				success = true,
				statusCode = 200,
				message = null,
				data = null
			};
		}

		private UserDTO ToDTO(UserModel user)
		{
			return new UserDTO
			{
				Id = user.Id,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Username = user.Username,
				Email = user.Email,
				CurrencyQuantity = user.CurrencyQuantity,
				Language = user.Language,
				Role = user.Role,
				Status = user.Status,
				RankId = user.RankId
			};
		}
	}
}
