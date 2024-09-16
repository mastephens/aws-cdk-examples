using Amazon.DynamoDBv2.DataModel;

namespace SampleDynamoBlogAspNetApi.Models
{
  [DynamoDBTable("blogs")]
  public class Blog
  {
    [DynamoDBHashKey]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }
    public DateTime CreatedTimestamp { get; set; }
  }
}
