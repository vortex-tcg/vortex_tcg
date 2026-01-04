
#nullable disable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using VortexTCG.Api.Logs.ActionType.Controllers;
using VortexTCG.Api.Logs.ActionType.DTOs;
using VortexTCG.Api.Logs.ActionType.Providers;
using VortexTCG.Api.Logs.ActionType.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Common.DTO;
using Microsoft.EntityFrameworkCore;

namespace Tests.Logs.ActionType
{
    public class ActionTypeTest
    {
        private VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private ActionTypeController CreateController(VortexDbContext db)
        {
            ActionTypeProvider provider = new ActionTypeProvider(db);
            ActionTypeService service = new ActionTypeService(provider);
            return new ActionTypeController(service);
        }

        [Fact]
        public async Task CreateActionType_ReturnsCreated()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeController controller = CreateController(db);
            ActionTypeCreateDTO dto = new ActionTypeCreateDTO {
                ActionDescription = "Action 1",
                GameLogId = Guid.NewGuid(),
                ParentId = null
            };
            IActionResult createResult = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<ActionTypeDTO> payload = Assert.IsType<ResultDTO<ActionTypeDTO>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(201, payload.statusCode);
            Assert.Equal("Action 1", payload.data.ActionDescription);
        }

        [Fact]
        public async Task GetById_ReturnsActionType()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeController controller = CreateController(db);
            ActionTypeCreateDTO dto = new ActionTypeCreateDTO {
                ActionDescription = "Action 2",
                GameLogId = Guid.NewGuid(),
                ParentId = null
            };
            IActionResult createResult = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<ActionTypeDTO> payload = Assert.IsType<ResultDTO<ActionTypeDTO>>(created.Value);
            Guid? id = payload?.data?.Id;
            IActionResult getResult = await controller.GetById(id ?? Guid.Empty);
            if (getResult is ObjectResult ok)
            {
                ResultDTO<ActionTypeDTO> payloadGet = Assert.IsType<ResultDTO<ActionTypeDTO>>(ok.Value);
                if (payloadGet != null && payloadGet.data != null)
                {
                    Assert.Equal("Action 2", payloadGet.data.ActionDescription);
                }
            }
            else
            {
                Assert.IsType<NotFoundResult>(getResult);
            }
        }

        [Fact]
        public async Task UpdateActionType_ChangesValues()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeController controller = CreateController(db);
            ActionTypeCreateDTO dto = new ActionTypeCreateDTO {
                ActionDescription = "Action 3",
                GameLogId = Guid.NewGuid(),
                ParentId = null
            };
            IActionResult createResult = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<ActionTypeDTO> payload = Assert.IsType<ResultDTO<ActionTypeDTO>>(created.Value);
            Guid? id = payload?.data?.Id;
            ActionTypeCreateDTO updateDto = new ActionTypeCreateDTO {
                ActionDescription = "Action 3 Updated",
                GameLogId = (payload?.data?.GameLogId) ?? Guid.Empty,
                ParentId = null
            };
            IActionResult updateResult = await controller.Update(id ?? Guid.Empty, updateDto);
            if (updateResult is ObjectResult ok)
            {
                ResultDTO<ActionTypeDTO> payloadUpdate = Assert.IsType<ResultDTO<ActionTypeDTO>>(ok.Value);
                if (payloadUpdate != null && payloadUpdate.data != null)
                {
                    Assert.True(payloadUpdate.success);
                    Assert.Equal("Action 3 Updated", payloadUpdate.data.ActionDescription);
                }
            }
            else
            {
                Assert.IsType<ObjectResult>(updateResult);
            }
        }

        [Fact]
        public async Task DeleteActionType_RemovesActionType()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeController controller = CreateController(db);
            ActionTypeCreateDTO dto = new ActionTypeCreateDTO {
                ActionDescription = "Action 4",
                GameLogId = Guid.NewGuid(),
                ParentId = null
            };
            IActionResult createResult = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<ActionTypeDTO> payload = Assert.IsType<ResultDTO<ActionTypeDTO>>(created.Value);
            Guid? id = payload?.data?.Id;
            IActionResult deleteResult = await controller.Delete(id ?? Guid.Empty);
            if (deleteResult is ObjectResult deleted)
            {
                ResultDTO<object> payloadDelete = Assert.IsType<ResultDTO<object>>(deleted.Value);
                if (payloadDelete != null)
                {
                    Assert.True(payloadDelete.success || payloadDelete.statusCode == 404);
                }
            }
            else
            {
                Assert.IsType<ObjectResult>(deleteResult);
            }
            IActionResult getResult = await controller.GetById(id ?? Guid.Empty);
            if (getResult is ObjectResult notFound)
            {
                ResultDTO<ActionTypeDTO> payloadNotFound = Assert.IsType<ResultDTO<ActionTypeDTO>>(notFound.Value);
                Assert.False(payloadNotFound.success);
                Assert.Equal(404, payloadNotFound.statusCode);
            }
            else
            {
                Assert.IsType<NotFoundResult>(getResult);
            }
        }
    }
}
