using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VortexTCG.Api.Card.Controllers;
using VortexTCG.Api.Card.DTOs;
using VortexTCG.Api.Card.Services;
using VortexTCG.Common.DTO;
using Xunit;

namespace VortexTCG.Tests.Api.Card.Controllers
{
    public class CardControllerTest
    {
        [Fact]
        public async Task GetAll_ReturnsOk_WithList()
        {
            var mockService = new Mock<CardService>(MockBehavior.Strict, null!);
            mockService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new ResultDTO<CardDTO[]>
                       {
                           success = true,
                           statusCode = 200,
                           data = new[] { new CardDTO { Id = Guid.NewGuid(), Name = "Test" } }
                       });

            var controller = new CardController(mockService.Object);

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<CardDTO[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
            Assert.Single(payload.data!);
        }

        [Fact]
        public async Task GetById_NotFound_WhenMissing()
        {
            var mockService = new Mock<CardService>(MockBehavior.Strict, null!);
            mockService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new ResultDTO<CardDTO>
                       {
                           success = false,
                           statusCode = 404,
                           message = "Carte non trouvée"
                       });

            var controller = new CardController(mockService.Object);

            var result = await controller.GetById(Guid.NewGuid());

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<CardDTO>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task GetById_Ok_WhenFound()
        {
            var dto = new CardDTO { Id = Guid.NewGuid(), Name = "Found" };
            var mockService = new Mock<CardService>(MockBehavior.Strict, null!);
            mockService.Setup(s => s.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new ResultDTO<CardDTO>
                       {
                           success = true,
                           statusCode = 200,
                           data = dto
                       });

            var controller = new CardController(mockService.Object);

            var result = await controller.GetById(dto.Id);

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<CardDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal("Found", payload.data!.Name);
        }
    }
}
