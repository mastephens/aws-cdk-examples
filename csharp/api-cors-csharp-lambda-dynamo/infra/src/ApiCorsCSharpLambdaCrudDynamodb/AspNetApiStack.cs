using System;
using System.Collections.Generic;
using System.Net;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Cognito;
using Constructs;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

namespace ApiCorsCSharpLambdaCrudDynamodb;

public class AspNetApiStack : Stack
{
  internal AspNetApiStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
  {
    string tableName = "blogs-v2";
    // must match case of primary key on Blog model
    string primaryKey = "Id";
    string restApiName = "Blogs AspNet API Service";
    string apiName = "blogsAspNetApi";

    var dynamoTable = new Table(this, tableName, new TableProps()
    {
      PartitionKey = new Attribute()
      {
        Name = primaryKey,
        Type = AttributeType.STRING
      },
      TableName = tableName,
      /**
       *  The default removal policy is RETAIN, which means that cdk destroy will not attempt to delete
       * the new table, and it will remain in your account until manually deleted. By setting the policy to
       * DESTROY, cdk destroy will delete the table (even if it has data in it)
       */
      RemovalPolicy = RemovalPolicy.DESTROY
    });

    // create environment variables that are needed by the Lambda functions
    var environmentVariables = new Dictionary<string, string>()
    {
       {"PRIMARY_KEY", primaryKey },
       {"TABLE_NAME", dynamoTable.TableName },
    };

    var blogServiceLambda = new Function(this, "blogFunction", new FunctionProps
    {
      Runtime = Runtime.DOTNET_6,
      Code = Code.FromAsset("../app/SampleDynamoBlogAspNetApi/src/SampleDynamoBlogAspNetApi/bin/Release/net6.0/linux-x64/"),
      Handler = "SampleDynamoBlogAspNetApi",
      Tracing = Tracing.ACTIVE, // enable X-Ray
      Environment = environmentVariables,
      Timeout = Duration.Seconds(30)
    });

    dynamoTable.GrantReadWriteData(blogServiceLambda);

    var pool = new UserPool(this, "blogsUserPool", new UserPoolProps()
    {
      SelfSignUpEnabled = true,
      SignInAliases = new SignInAliases()
      {
        Email = true
      }
    });

    var authorizer = new CognitoUserPoolsAuthorizer(this, "apiAuthorizer", new CognitoUserPoolsAuthorizerProps()
    {
      CognitoUserPools = new IUserPool[]{pool},
    });

    // Create an API Gateway resource for each of the CRUD operations
    var api = new RestApi(this, apiName, new RestApiProps()
    {
      RestApiName = restApiName
    });

    var items = api.Root.AddProxy(new ProxyResourceOptions
    {
      DefaultIntegration = new LambdaIntegration(blogServiceLambda),
      DefaultMethodOptions = new MethodOptions()
      {
        Authorizer = authorizer,
        AuthorizationType = AuthorizationType.COGNITO
      },
      // "false" will require explicitly adding methods on the `proxy` resource
      AnyMethod = true,
    });
    var swagger = api.Root.AddResource("swagger");
    var swaggerProxy = swagger.AddProxy(new ProxyResourceOptions()
    {
      DefaultIntegration = new LambdaIntegration(blogServiceLambda),
      DefaultMethodOptions = new MethodOptions(),
      AnyMethod = false
    });
    swaggerProxy.AddMethod("GET");
    AddCorsOptions(api.Root);
  }

  private void AddCorsOptions(Amazon.CDK.AWS.APIGateway.IResource apiResource)
  {
    apiResource.AddMethod("OPTIONS", new MockIntegration(
      new IntegrationOptions()
      {
        IntegrationResponses = new IntegrationResponse[] {
          new IntegrationResponse()
          {
            StatusCode = ((int)HttpStatusCode.OK).ToString(),
            ResponseParameters = new Dictionary<string, string>()
            {
              // Note the single quotes around the values in this collection, they are required
              { "method.response.header.Access-Control-Allow-Headers", "'Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token,X-Amz-User-Agent'" },
              { "method.response.header.Access-Control-Allow-Origin", "'*'" },
              { "method.response.header.Access-Control-Allow-Credentials", "'false'" },
              { "method.response.header.Access-Control-Allow-Methods", "'OPTIONS,GET,PUT,POST,DELETE'" }
            }
          }
        },
        PassthroughBehavior = PassthroughBehavior.NEVER,
        RequestTemplates = new Dictionary<string, string>()
        {
          { "application/json", "{\"statusCode\": 200}" }
        }
      }),
      new MethodOptions()
      {
        MethodResponses = new MethodResponse[]
        {
          new MethodResponse()
          {
            StatusCode = ((int)HttpStatusCode.OK).ToString(),
            ResponseParameters = new Dictionary<string, bool>()
            {
              { "method.response.header.Access-Control-Allow-Headers", true },
              { "method.response.header.Access-Control-Allow-Methods", true },
              { "method.response.header.Access-Control-Allow-Credentials", true },
              { "method.response.header.Access-Control-Allow-Origin", true },
            }
          }
        }
      });
  }
}
