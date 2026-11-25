using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using S3_API.Controllers;

namespace S3API.Tests;

public class S3ControllerTests
{
    [Fact]
    public void ApiCheck_ReturnsOk()
    {
        var r = new S3Controller().ApiCheck();
        Assert.Equal("API is Working", Assert.IsType<OkObjectResult>(r).Value);
    }

    [Fact]
    public async Task GetFile_ReturnsLastObjectContent()
    {
        var s3 = Substitute.For<IAmazonS3>();
        s3.ListBucketsAsync(Arg.Any<CancellationToken>()).Returns(new ListBucketsResponse { Buckets = new List<S3Bucket> { new() { BucketName = "b" } } });
        s3.ListObjectsAsync("b", Arg.Any<CancellationToken>()).Returns(new ListObjectsResponse { S3Objects = new List<S3Object> { new() { Key = "k" } } });
        s3.GetObjectAsync(Arg.Any<GetObjectRequest>(), Arg.Any<CancellationToken>())
          .Returns(_ => new GetObjectResponse { ResponseStream = new MemoryStream("hello"u8.ToArray()) });

        var r = await new S3Controller().GetFile(s3);
        Assert.Equal("hello", Assert.IsType<OkObjectResult>(r).Value);
    }

    [Fact]
    public async Task GetFile_NoBuckets_ReturnsEmpty()
    {
        var s3 = Substitute.For<IAmazonS3>();
        s3.ListBucketsAsync(Arg.Any<CancellationToken>()).Returns(new ListBucketsResponse { Buckets = new List<S3Bucket>() });
        var r = await new S3Controller().GetFile(s3);
        Assert.Equal("", Assert.IsType<OkObjectResult>(r).Value);
    }
}

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public ApiTests(WebApplicationFactory<Program> factory) => _factory = factory.WithWebHostBuilder(b => b.UseEnvironment("Development"));

    [Fact]
    public async Task HealthEndpoint_Responds()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var res = await client.GetAsync("/api/test/v1");
        Assert.True(res.StatusCode is HttpStatusCode.OK or HttpStatusCode.TemporaryRedirect, $"{(int)res.StatusCode} {await res.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task Swagger_IsServedInDevelopment()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var res = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.True(res.StatusCode is HttpStatusCode.OK or HttpStatusCode.TemporaryRedirect, $"{(int)res.StatusCode} {await res.Content.ReadAsStringAsync()}");
    }
}
