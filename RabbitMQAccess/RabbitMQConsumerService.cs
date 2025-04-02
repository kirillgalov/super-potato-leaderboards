using System.Collections.Concurrent;
using System.Text.Json;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace RabbitMQAccess;

public class RabbitMQConsumerService : BackgroundService, IEventConsumer
{
    private const int RetryConnectionInterval = 5_000;
    private const int ConsumeInterval = 5_000;

    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProducerService> _logger;
    private readonly ConcurrentQueue<ScoresAdded> _scoresAdded = new ConcurrentQueue<ScoresAdded>();


    public RabbitMQConsumerService(IConfiguration configuration, ILogger<RabbitMQProducerService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<List<ScoresAdded>> GetNewScoresAsync(CancellationToken stoppingToken)
    {
        List<ScoresAdded> newScores = [];

        while (_scoresAdded.TryDequeue(out ScoresAdded newScore))
        {
            newScores.Add(newScore);
        }

        return Task.FromResult(newScores);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var connectionFactory = new ConnectionFactory()
                {
                    HostName = _configuration.GetRabbitMQHostName(),
                };

                await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken: stoppingToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: QueueNames.ScoresAdded,
                    exclusive: false,
                    cancellationToken: stoppingToken);

                await ConsumeAsync(channel, stoppingToken);

            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            await Task.Delay(RetryConnectionInterval, stoppingToken);
        }

    }

    private async Task ConsumeAsync(IChannel channel, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            long messageCount = await channel.MessageCountAsync(QueueNames.ScoresAdded, stoppingToken);

            for (int i = 0; i < messageCount; i++)
            {
                BasicGetResult? result = await channel.BasicGetAsync(
                    queue: QueueNames.ScoresAdded,
                    autoAck: true,
                    cancellationToken: stoppingToken);

                if (result == null)
                    break;

                ScoresAdded? score = JsonSerializer.Deserialize<ScoresAdded>(result.Body.Span);
                if (score != null)
                    _scoresAdded.Enqueue(score);
            }

            await Task.Delay(ConsumeInterval, stoppingToken);
        }
    }
}