using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMQAccess;

public static class RabbitMQClientConfigure
{
    public static string GetRabbitMQHostName(this IConfiguration configuration)
    {
        return configuration["RabbitMQ:HostName"] ?? throw new Exception("RabbitMQ Host Name is missing");
    }

    public static void AddRabbitMQProducer(this IServiceCollection services)
    {
        services.AddSingleton<RabbitMQProducerService>();
        services.AddHostedService<RabbitMQProducerService>(sp => sp.GetRequiredService<RabbitMQProducerService>());
        services.AddSingleton<IEventProducer>(sp => sp.GetRequiredService<RabbitMQProducerService>());
    }

    public static void AddRabbitMQConsumer(this IServiceCollection services)
    {
        services.AddSingleton<RabbitMQConsumerService>();
        services.AddHostedService<RabbitMQConsumerService>(sp => sp.GetRequiredService<RabbitMQConsumerService>());
        services.AddSingleton<IEventConsumer>(sp => sp.GetRequiredService<RabbitMQConsumerService>());
    }
}