using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace VortexTCG.Api.Card.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucket;

        public ImagesController(IAmazonS3 s3, IConfiguration config)
        {
            _s3 = s3;
            _bucket = config["AWS:S3:Bucket"]!;
        }

        [HttpGet("{*key}")]
        public async Task<IActionResult> GetImage(string key, CancellationToken ct)
        {
            // TODO: check droits user ?

            GetObjectResponse obj;
            try
            {
                obj = await _s3.GetObjectAsync(new GetObjectRequest
                {
                    BucketName = _bucket,
                    Key = "card/"+ key
                }, ct);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            string contentType = string.IsNullOrWhiteSpace(obj.Headers.ContentType)
                ? "application/octet-stream"
                : obj.Headers.ContentType;
            return File(obj.ResponseStream, contentType);
        }
    }
}
