using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace S3_API.Controllers
{
    [Route("api")]
    [ApiController]
    public class S3Controller : ControllerBase
    {
        [Route("test/v1")]
        [HttpGet]
        public IActionResult ApiCheck()
        {
            return Ok("API is Working");
        }

        [Route("getFile")]
        [HttpGet]
        public async Task<IActionResult> GetFile([FromServices] IAmazonS3 s3Client)
        {
            var buckets = await s3Client.ListBucketsAsync();
            string objectContent = "";
            foreach(var bucket in buckets.Buckets)
            {
                var objects = await s3Client.ListObjectsAsync(bucket.BucketName);
                if (objects != null)
                foreach(var s3objects in objects.S3Objects)
                {
                    var objectResponse = await s3Client.GetObjectAsync(new GetObjectRequest
                    {
                        BucketName = bucket.BucketName,
                        Key = s3objects.Key
                    });
                    using var reader = new StreamReader(objectResponse.ResponseStream, Encoding.UTF8);
                    objectContent = await reader.ReadToEndAsync();
                }
            }
            return Ok(objectContent);
        }
    }
}
