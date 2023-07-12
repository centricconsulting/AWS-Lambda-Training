using Amazon.DynamoDBv2.Model;

namespace Models;
    public class ApiRequest
    {
        public string? operation { get; set; }
        public Payload? payload { get; set; }
    }

    public class Payload
    {
        public Dictionary<string, AttributeValue>? Key { get; set; }
        public Dictionary<string, AttributeValue>? Item { get; set; }
        public string? UpdateExpression {get; set;}
        public Dictionary<string, AttributeValue>? ExpressionAttributeValues {get; set;}
    }


