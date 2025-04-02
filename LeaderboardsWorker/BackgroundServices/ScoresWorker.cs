using Domain;
using Microsoft.Extensions.Options;

namespace LeaderboardsWorker.BackgroundServices;

public class ScoresWorker : BackgroundService
{
    private readonly ILogger<ScoresWorker> _logger;
    private readonly IPlayerScoreRepository _playerScoreRepository;
    private readonly IEventConsumer _consumer;
    private readonly IOptionsMonitor<WorkerSettings> _settings;
    private const int RetryInterval = 5000;

    public ScoresWorker(ILogger<ScoresWorker> logger, IPlayerScoreRepository playerScoreRepository, IEventConsumer consumer, IOptionsMonitor<WorkerSettings> settings)
    {
        _logger = logger;
        _playerScoreRepository = playerScoreRepository;
        _consumer = consumer;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var newScores = await _consumer.GetNewScoresAsync(stoppingToken);

                if (newScores is {Count: > 0})
                    await _playerScoreRepository.UpdateScoresAsync(newScores, stoppingToken);

                await Task.Delay(_settings.CurrentValue.IntervalMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            await Task.Delay(RetryInterval, stoppingToken);
        }
    }

}