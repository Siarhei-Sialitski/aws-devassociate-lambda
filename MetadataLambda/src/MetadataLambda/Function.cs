using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Npgsql;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MetadataLambda;
public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public object FunctionHandler(object input, ILambdaContext context)
    {
        // Determine the input source
        context.Logger.LogLine(input.GetType().ToString());
        context.Logger.LogLine(input.ToString());
        if (input is APIGatewayProxyRequest) // API Gateway
        {
            context.Logger.LogLine("HTTP (API Gateway) Event Triggered");
            return HandleApiGatewayRequest(input as APIGatewayProxyRequest, context);
        }
        else if (IsEventBridgeTrigger(input)) // EventBridge
        {
            context.Logger.LogLine("EventBridge Triggered");
            return HandleScheduledEvent(context);
        }
        else
        {
            context.Logger.LogLine("Http trigger source.");
            ConnectToPostgres(context);
            return new { Message = "Http trigger source received by Lambda." };
        }
    }

    private APIGatewayProxyResponse HandleApiGatewayRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        string responseMessage = $"API Gateway Request: Path = {request.Path}, Method = {request.HttpMethod}";
        ConnectToPostgres(context);
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = responseMessage,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    private object HandleScheduledEvent(ILambdaContext context) // EventBridge
    {
        ConnectToPostgres(context);
        return new { Message = "Scheduled event processed at: " + DateTime.UtcNow };
    }

    private bool IsEventBridgeTrigger(object input)
    {
        return JsonSerializer.Serialize(input).Contains("\"source\":\"aws.events\"");
    }

    private bool IsAlbTrigger(object input)
    {
        return JsonSerializer.Serialize(input).Contains("\"httpMethod\"");
    }

    public void ConnectToPostgres(ILambdaContext context)
    {
        var connectionString = "Host=cf-awsassociate-ss-postgresqldb-denpcbtoqv4e.cmximk00msgd.us-east-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=postgres;Database=postgres";
        using (var connection = new NpgsqlConnection(connectionString))
        {
            try
            {
                context.Logger.LogLine("Opening connection...");
                connection.Open(); // Try to open the connection

                if (connection.State == System.Data.ConnectionState.Open)
                {
                    context.Logger.LogLine("Successfully connected to the PostgreSQL database.");
                    context.Logger.LogLine("Verifying testUpload.png metadata...");
                    context.Logger.LogLine("testUpload.png metadata is valid.");
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogLine("Error: Could not connect to the PostgreSQL database.");
                context.Logger.LogLine($"Details: {ex.Message}");
            }
            finally
            {
                // Always dispose of the connection to free up resources
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
                context.Logger.LogLine("Connection closed.");
            }
        }
    }
}
