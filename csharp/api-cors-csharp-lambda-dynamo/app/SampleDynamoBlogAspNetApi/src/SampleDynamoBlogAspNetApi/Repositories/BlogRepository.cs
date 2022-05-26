using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SampleDynamoBlogAspNetApi.Models;

namespace SampleDynamoBlogAspNetApi.Repositories
{
  public class BlogRepository : IBlogRepository
  {
    private readonly ILogger<BlogRepository> _logger;
    private const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "TABLE_NAME";
    private const string PRIMARYKEY_ENVIRONMENT_VARIABLE_LOOKUP = "PRIMARY_KEY";
    private readonly string _tableName;
    private readonly string _primaryKey;
    private readonly IDynamoDBContext _ddbContext;

    public BlogRepository(ILogger<BlogRepository> logger, IAmazonDynamoDB ddbClient)
    {
      _logger = logger;
      // Check to see if a table name was passed in through environment variables and if so
      // add the table mapping.
      _tableName = Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
      _primaryKey = Environment.GetEnvironmentVariable(PRIMARYKEY_ENVIRONMENT_VARIABLE_LOOKUP);
      if (!string.IsNullOrEmpty(_tableName))
      {
        AWSConfigsDynamoDB.Context.TypeMappings[typeof(Blog)] = new Amazon.Util.TypeMapping(typeof(Blog), _tableName);
      }

      var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
      this._ddbContext = new DynamoDBContext(ddbClient, config);
    }
    public async Task<List<Blog>> GetAll()
    {
      var blogs = await this._ddbContext.ScanAsync<Blog>(new List<ScanCondition>()).GetRemainingAsync();
      return blogs;
    }

    public async Task<Blog> GetOne(string id)
    {
      if (string.IsNullOrEmpty(id))
      {
        throw new Exception("Required Parameter Id is missing");
      }

      var blog = await this._ddbContext.LoadAsync<Blog>(id);

      return blog;
    }

    public async Task<Blog> Create(Blog blog)
    {
      if (blog == null)
      {
        throw new Exception("Required Parameter blog is missing");
      }

      if (!string.IsNullOrEmpty(blog.Id))
      {
        throw new Exception("Id can not be set on create blog");
      }

      blog.Id = Guid.NewGuid().ToString();
      blog.CreatedTimestamp = DateTime.UtcNow;
      blog.ModifiedTimestamp = blog.CreatedTimestamp;
      await this._ddbContext.SaveAsync<Blog>(blog);
      var newBlog = await this._ddbContext.LoadAsync<Blog>(blog.Id);
      return newBlog;
    }

    public async Task<Blog> Update(string id, Blog blog)
    {
      if (string.IsNullOrEmpty(id))
      {
        throw new Exception("Required Parameter Id is missing");
      }

      if (blog == null || string.IsNullOrWhiteSpace(blog.Id) || string.IsNullOrWhiteSpace(blog.Name))
      {
        throw new Exception("Required Parameter blog is missing");
      }

      var existingBlog = await this._ddbContext.LoadAsync<Blog>(blog.Id);
      if (existingBlog == null)
      {
        throw new Exception("Blog does not exist");
      }

      blog.ModifiedTimestamp = DateTime.UtcNow;
      await this._ddbContext.SaveAsync<Blog>(blog);
      var newBlog = await this._ddbContext.LoadAsync<Blog>(blog.Id);
      return newBlog;
    }

    public async Task<string> Delete(string id)
    {
      if (string.IsNullOrEmpty(id))
      {
        throw new Exception("Required Parameter Id is missing");
      }

      var existingBlog = await this._ddbContext.LoadAsync<Blog>(id);
      if (existingBlog == null)
      {
        throw new Exception("Blog does not exist");
      }

      await this._ddbContext.DeleteAsync<Blog>(id);

      return id;
    }
  }
}
