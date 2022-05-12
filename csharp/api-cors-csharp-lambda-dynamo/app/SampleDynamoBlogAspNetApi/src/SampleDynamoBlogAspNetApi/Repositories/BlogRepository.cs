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
  }
}
