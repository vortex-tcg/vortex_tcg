#nullable disable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using VortexTCG.Api.User.Controllers;
using VortexTCG.Api.User.DTOs;
using VortexTCG.Api.User.Providers;
using VortexTCG.Api.User.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using RoleEnum = VortexTCG.DataAccess.Models.Role;
using UserModel = VortexTCG.DataAccess.Models.User;
using VortexTCG.Common.DTO;
using Microsoft.EntityFrameworkCore;

namespace VortexTCG.Tests.Api.User.Controllers
{
	public class UserControllerTest
	{
		private VortexDbContext CreateDb()
		{
			DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			return new VortexDbContext(options);
		}

		private UserController CreateController(VortexDbContext db)
		{
			UserProvider provider = new UserProvider(db);
			UserService service = new UserService(provider);
			return new UserController(service);
		}

		[Fact]
		public async Task CreateUser_ReturnsCreated()
		{
			using VortexDbContext db = CreateDb();
			UserController controller = CreateController(db);
			UserCreateDTO dto = new UserCreateDTO {
				FirstName = "John",
				LastName = "Doe",
				Username = "johndoe",
				Password = "pass",
				Email = "john@doe.com",
				CurrencyQuantity = 100,
				Language = "fr",
				Role = RoleEnum.USER,
				Status = UserStatus.CONNECTED,
				RankId = null
			};
			IActionResult result = await controller.Add(dto);
			ObjectResult created = Assert.IsType<ObjectResult>(result);
			ResultDTO<UserDTO> payload = Assert.IsType<ResultDTO<UserDTO>>(created.Value);
			Assert.True(payload.success);
			Assert.Equal(201, payload.statusCode);
			Assert.Equal("johndoe", payload.data.Username);
		}

		[Fact]
		public async Task CreateUser_DuplicateUsername_ReturnsConflict()
		{
			using VortexDbContext db = CreateDb();
			UserController controller = CreateController(db);
			UserCreateDTO dto = new UserCreateDTO {
				FirstName = "Jane",
				LastName = "Smith",
				Username = "janesmith",
				Password = "pass",
				Email = "jane@smith.com",
				CurrencyQuantity = 50,
				Language = "en",
				Role = Role.USER,
				Status = UserStatus.CONNECTED,
				RankId = null
			};
			await controller.Add(dto);
			IActionResult result = await controller.Add(dto);
			ObjectResult conflict = Assert.IsType<ObjectResult>(result);
			ResultDTO<UserDTO> payload = Assert.IsType<ResultDTO<UserDTO>>(conflict.Value);
			Assert.False(payload.success);
			Assert.Equal(409, payload.statusCode);
		}

		[Fact]
		public async Task GetById_ReturnsUser()
		{
			using VortexDbContext db = CreateDb();
			UserController controller = CreateController(db);
			UserCreateDTO dto = new UserCreateDTO {
				FirstName = "Alice",
				LastName = "Wonder",
				Username = "alicew",
				Password = "pass",
				Email = "alice@wonder.com",
				CurrencyQuantity = 200,
				Language = "en",
				Role = Role.ADMIN,
				Status = UserStatus.CONNECTED,
				RankId = null
			};
			IActionResult createResult = await controller.Add(dto);
			ObjectResult created = Assert.IsType<ObjectResult>(createResult);
			ResultDTO<UserDTO> payloadCreate = Assert.IsType<ResultDTO<UserDTO>>(created.Value);
			IActionResult getResult = await controller.GetById(payloadCreate.data.Id);
			ObjectResult ok = Assert.IsType<ObjectResult>(getResult);
			ResultDTO<UserDTO> payload = Assert.IsType<ResultDTO<UserDTO>>(ok.Value);
			Assert.Equal("Alice", payload.data.FirstName);
			Assert.Equal("alicew", payload.data.Username);
		}

		[Fact]
		public async Task UpdateUser_ChangesValues()
		{
			using VortexDbContext db = CreateDb();
			UserController controller = CreateController(db);
			UserCreateDTO dto = new UserCreateDTO {
				FirstName = "Bob",
				LastName = "Builder",
				Username = "bobbuilder",
				Password = "pass",
				Email = "bob@builder.com",
				CurrencyQuantity = 300,
				Language = "en",
				Role = Role.USER,
				Status = UserStatus.CONNECTED,
				RankId = null
			};
			IActionResult createResult = await controller.Add(dto);
			ObjectResult created = Assert.IsType<ObjectResult>(createResult);
			ResultDTO<UserDTO> payloadCreate = Assert.IsType<ResultDTO<UserDTO>>(created.Value);
			UserCreateDTO updateDto = new UserCreateDTO {
				FirstName = "Robert",
				LastName = "Builder",
				Username = "robertbuilder",
				Password = "newpass",
				Email = "robert@builder.com",
				CurrencyQuantity = 400,
				Language = "fr",
				Role = Role.ADMIN,
				Status = UserStatus.CONNECTED,
				RankId = null
			};
			IActionResult updateResult = await controller.Update(payloadCreate.data.Id, updateDto);
			ObjectResult ok = Assert.IsType<ObjectResult>(updateResult);
			ResultDTO<UserDTO> payload = Assert.IsType<ResultDTO<UserDTO>>(ok.Value);
			Assert.True(payload.success);
			Assert.Equal("Robert", payload.data.FirstName);
			Assert.Equal("robertbuilder", payload.data.Username);
		}

		[Fact]
		public async Task DeleteUser_RemovesUser()
		{
			using VortexDbContext db = CreateDb();
			UserController controller = CreateController(db);
			UserCreateDTO dto = new UserCreateDTO {
				FirstName = "Charlie",
				LastName = "Chocolate",
				Username = "charliec",
				Password = "pass",
				Email = "charlie@choco.com",
				CurrencyQuantity = 500,
				Language = "en",
				Role = Role.USER,
				Status = UserStatus.CONNECTED,
				RankId = null
			};
			IActionResult createResult = await controller.Add(dto);
			ObjectResult created = Assert.IsType<ObjectResult>(createResult);
			ResultDTO<UserDTO> payloadCreate = Assert.IsType<ResultDTO<UserDTO>>(created.Value);
			IActionResult deleteResult = await controller.Delete(payloadCreate.data.Id);
			ObjectResult deleted = Assert.IsType<ObjectResult>(deleteResult);
			ResultDTO<object> payload = Assert.IsType<ResultDTO<object>>(deleted.Value);
			Assert.False(payload.success == false && payload.statusCode == 404);
			IActionResult getResult = await controller.GetById(payloadCreate.data.Id);
			if (getResult is NotFoundResult)
			{
				Assert.IsType<NotFoundResult>(getResult);
			}
			else
			{
				ObjectResult ok = Assert.IsType<ObjectResult>(getResult);
				ResultDTO<UserDTO> payloadGet = Assert.IsType<ResultDTO<UserDTO>>(ok.Value);
				Assert.True(payloadGet == null || payloadGet.data == null);
			}
		}
	}
}
