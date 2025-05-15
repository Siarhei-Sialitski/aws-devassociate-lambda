using Amazon.SimpleNotificationService;
using Microsoft.Extensions.DependencyInjection;
using WebApi;

namespace SqsLamda;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add any required services here
        // For example, if you need to add a database context or other services
        // services.AddDbContext<MyDbContext>(options => ...);
        services.AddAWSService<IAmazonSimpleNotificationService>();
        services.AddSingleton<ISnsRepository, SnsRepository>();
    }
}
