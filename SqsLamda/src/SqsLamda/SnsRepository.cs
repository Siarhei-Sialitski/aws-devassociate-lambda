using System.Text;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace WebApi;

public interface ISnsRepository
{
    Task PublishMessage(ImageMetaInfo imageMetaInfo);
}

public class SnsRepository : ISnsRepository
{
    private readonly IAmazonSimpleNotificationService _client;

    public SnsRepository(IAmazonSimpleNotificationService client)
    {
        _client = client;
    }

    public async Task PublishMessage(ImageMetaInfo imageMetaInfo)
    {
        var topics = await _client.ListTopicsAsync();
        var topicArn = topics.Topics.FirstOrDefault(x => x.TopicArn.Contains("sns-aws-associate-ss-uploads-notification-topic"))?.TopicArn;

        if (topicArn == null)
        {
            throw new Exception("Topic not found");
        }

        var stringBuidler = new StringBuilder();

        stringBuidler.AppendLine("The image was uploaded successfully.");
        stringBuidler.AppendLine($"Image Name: {imageMetaInfo.ImageName}");
        stringBuidler.AppendLine($"Image Name: {imageMetaInfo.FileExtension}");
        stringBuidler.AppendLine($"Image Size: {imageMetaInfo.ContentLength}");
        stringBuidler.AppendLine($"Image Last Modified: {imageMetaInfo.LastModified}");
        stringBuidler.AppendLine($"Image URL: https://s3-awsassociate-siarhei-sialitski.s3.amazonaws.com/{imageMetaInfo.ImageName}");

        var request = new PublishRequest
        {
            TopicArn = topicArn,
            Message = stringBuidler.ToString()
        };

        var response = await _client.PublishAsync(request);
    }
}

public record ImageMetaInfo(string ImageName, long ContentLength, DateTime? LastModified, string FileExtension);
