using System.Text.Json;
using System.Threading.Channels;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace RabbitMQAccess;

public class RabbitMQProducerService : BackgroundService, IEventProducer
{
    private const int RabbitMQReconnectInterval = 5_000;

    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProducerService> _logger;
    private readonly Channel<ScoresAdded> _scoresAddedChannel;
    public RabbitMQProducerService(IConfiguration configuration, ILogger<RabbitMQProducerService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _scoresAddedChannel = Channel.CreateUnbounded<ScoresAdded>(new UnboundedChannelOptions { SingleReader = true });
    }

    public async Task PublishNewScoreAsync(ScoresAdded scoresAdded, CancellationToken token)
    {
        await _scoresAddedChannel.Writer.WriteAsync(scoresAdded, token);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var connectionFactory = new ConnectionFactory()
                {
                    HostName = _configuration.GetRabbitMQHostName()
                };

                _logger.LogInformation("Starting RabbitMQ client {hostname}", connectionFactory.HostName);
                await using var connection = await connectionFactory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                await channel.QueueDeclareAsync(
                    queue: QueueNames.ScoresAdded,
                    exclusive: false,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("RabbitMQ client started");

                await foreach (ScoresAdded scoresAdded in _scoresAddedChannel.Reader.ReadAllAsync(stoppingToken))
                {
                    ReadOnlyMemory<byte> body = JsonSerializer.SerializeToUtf8Bytes(scoresAdded);
                    await channel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: QueueNames.ScoresAdded,
                        body: body,
                        cancellationToken: stoppingToken);
                }

            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (BrokerUnreachableException)
            {
                _logger.LogWarning("RabbitMQ client broker is unreachable. Try again after {interval}ms", RabbitMQReconnectInterval);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}