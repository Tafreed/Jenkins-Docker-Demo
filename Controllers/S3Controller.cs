using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using S3_API.Config;
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
        public async Task<IActionResult> GetFile()
        {
            var s3Client = new AmazonS3Client(Constants.AccessId,Constants.SecretId,Amazon.RegionEndpoint.APSouth1);
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
                    var bytes = new byte[objectResponse.ResponseStream.Length];
                    objectResponse.ResponseStream.Read(bytes, 0, bytes.Length);
                    objectContent = Encoding.UTF8.GetString(bytes);
                }
            }
            return Ok(objectContent);
        }
    }
}
