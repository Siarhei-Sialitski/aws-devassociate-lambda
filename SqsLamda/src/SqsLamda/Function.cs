using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using WebApi;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SqsLamda;
public class Function
{

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {}


    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="evnt">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        var sns = new Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceClient();
        var snsRepo = new SnsRepository(sns);
        var topics = sns.ListTopicsAsync();
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(snsRepo, message, context);
        }
    }

    private async Task ProcessMessageAsync(ISnsRepository repo, SQSEvent.SQSMessage message, ILambdaContext context)
    {
        var snsMessage = JsonSerializer.Deserialize<ImageMetaInfo>(message.Body);
        context.Logger.LogInformation($"Processed message {message.Body}");
        await repo.PublishMessage(snsMessage);
        // TODO: Do interesting work based on the new message
        await Task.CompletedTask;
    }
}