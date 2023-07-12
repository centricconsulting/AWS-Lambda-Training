using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Models;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaApiGateway;

public class Function
{
    private AmazonDynamoDBClient ddbClient =  new AmazonDynamoDBClient(region: Amazon.RegionEndpoint.USEast2);
    private string tablename = "lambda-apigateway";
  
    /// <summary>
    /// A function that will receive an API request in order to update a no sql database
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(ApiRequest apiRequest, ILambdaContext context)
    {
        try
        {
            var apiRequestBody = JsonSerializer.Serialize(apiRequest);
            string? body = null;
            bool missingPayload = false;

            if(apiRequest.operation == "echo")
            {
                body = apiRequestBody;
            }
            else if(apiRequest.payload == null)
            {
                missingPayload = true;
            }        
            else
            {
                switch (apiRequest.operation)
                {
                    case "create":
                        var putItemRequest = new PutItemRequest
                        {
                            TableName = tablename,
                            Item = apiRequest.payload.Item
                        };
                        await ddbClient.PutItemAsync(putItemRequest);
                        body = "Item created";
                        break;
                    case "read":
                        var getItemRequest = new GetItemRequest 
                        {
                            TableName = tablename,
                            Key = apiRequest.payload.Key
                        };
                        var itemResponse = await ddbClient.GetItemAsync(getItemRequest);
                        var itemResponseJson = JsonSerializer.Serialize(itemResponse);
                        LambdaLogger.Log("itemResponse:  " + itemResponseJson);
                        Dictionary<string, AttributeValue> item = itemResponse.Item;
                        body = JsonSerializer.Serialize(item);
                        break;
                    case "update":
                        var updateItemRequest = new UpdateItemRequest
                        {
                            TableName = tablename,
                            Key = apiRequest.payload.Key,
                            UpdateExpression = apiRequest.payload.UpdateExpression,
                            ExpressionAttributeValues = apiRequest.payload.ExpressionAttributeValues
                        };
                        var updateItemRequestJson = JsonSerializer.Serialize(updateItemRequest);
                        LambdaLogger.Log("updateItemRequest:  " + updateItemRequestJson);
                        var updateResponse = await ddbClient.UpdateItemAsync(updateItemRequest);
                        body = "Item Updated";
                        break;
                    case "delete":
                        var deleteItemRequest = new DeleteItemRequest
                        {
                            TableName = tablename,
                            Key = apiRequest.payload.Key
                        };
                        await ddbClient.DeleteItemAsync(deleteItemRequest);
                        body = "Item Deleted";
                        break;
                }
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = missingPayload ? 400 : 200,
                Body = body
            };
        }
        catch(Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = ex.Message
            };
        }
    }
}
