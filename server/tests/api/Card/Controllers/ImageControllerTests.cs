using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using VortexTCG.Api.Card.Controllers;
using Xunit;

namespace VortexTCG.Tests.Api.Card.Controllers
{
    public class ImageControllerTests
    {
        private static IConfiguration CreateConfig(string bucket = "test-bucket")
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["AWS:S3:Bucket"] = bucket })
                .Build();
        }

        [Fact]
        public async Task GetImage_ReturnsFile_WhenFound()
        {
            Mock<IAmazonS3> s3Mock = new Mock<IAmazonS3>();
            GetObjectResponse response = new GetObjectResponse
            {
                ResponseStream = new MemoryStream(new byte[] { 1, 2, 3 })
            };
            s3Mock.Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            ImagesController controller = new ImagesController(s3Mock.Object, CreateConfig());

            IActionResult result = await controller.GetImage("cards/image.png", CancellationToken.None);

            FileStreamResult fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
        }

        [Fact]
        public async Task GetImage_ReturnsNotFound_WhenS3Returns404()
        {
            Mock<IAmazonS3> s3Mock = new Mock<IAmazonS3>();
            AmazonS3Exception ex = new AmazonS3Exception("Not found") { StatusCode = System.Net.HttpStatusCode.NotFound };
            s3Mock.Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            ImagesController controller = new ImagesController(s3Mock.Object, CreateConfig());

            IActionResult result = await controller.GetImage("cards/missing.png", CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetImage_UsesS3ContentType_WhenProvided()
        {
            Mock<IAmazonS3> s3Mock = new Mock<IAmazonS3>();
            GetObjectResponse response = new GetObjectResponse
            {
                ResponseStream = new MemoryStream(new byte[] { 10, 20, 30 })
            };
            response.Headers.ContentType = "image/png";
            s3Mock.Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            ImagesController controller = new ImagesController(s3Mock.Object, CreateConfig());

            IActionResult result = await controller.GetImage("cards/image.png", CancellationToken.None);

            FileStreamResult fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("image/png", fileResult.ContentType);
        }
    }
}