using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Moq;
using SampleDynamoBlogAspNetApi.Controllers;
using SampleDynamoBlogAspNetApi.Models;
using SampleDynamoBlogAspNetApi.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SampleDynamoBlogAspNetApi.Tests
{
  public class GetAllBlogsTest
  {
    ILogger<BlogsController> _mockLogger;
    IAmazonDynamoDB _dynamoDbClient;

    public GetAllBlogsTest()
    {
      _mockLogger = new Mock<ILogger<BlogsController>>().Object;
      var mockDynamoDbClient = new Mock<IAmazonDynamoDB>();
      var scanResponse = new ScanResponse()
      {
        Items = new List<Dictionary<string, AttributeValue>>()
      };

      // sample mocking of ddb client api
      mockDynamoDbClient.Setup(m => m.ScanAsync(It.IsAny<string>(), It.IsAny<List<String>>(), It.IsAny<CancellationToken>()))
        .Returns(Task.FromResult(scanResponse));
      _dynamoDbClient = mockDynamoDbClient.Object;
    }

    [Fact(DisplayName = "Given a set of blogs stored in DDB, When I get all blogs, Then it returns all blogs.")]
    public async void GetBlogs_Returns()
    {
      var blogRepositoryMock = new Mock<IBlogRepository>();
      blogRepositoryMock.Setup(m => m.GetAll())
        .Returns(Task.FromResult(new List<Blog>() { new Blog() { Id = "1000" } }));

      var blogsController = new BlogsController(_mockLogger, blogRepositoryMock.Object);
      var results = await blogsController.GetBlogs();
      Assert.Collection(results, item => Assert.Equal("1000", item.Id));
      Assert.Single(results);
    }
  }
}
