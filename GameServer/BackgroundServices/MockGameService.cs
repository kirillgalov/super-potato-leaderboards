using Domain;

namespace GameServer.BackgroundServices;

public class MockGameService : IHostedService, IAsyncDisposable
{
    private readonly IEventProducer _eventProducer;
    private Timer _timer;

    public MockGameService(IEventProducer eventProducer)
    {
        _eventProducer = eventProducer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(CreateEvent, null, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(10));
        return Task.CompletedTask;
    }

    private void CreateEvent(object? state)
    {
        ScoresAdded newScore = new ScoresAdded
        {
            UserId = Random.Shared.Next(0, 10_000),
            Score = Random.Shared.Next(1, 101),
            CreatedAt = DateTime.UtcNow
        };
        _eventProducer.PublishNewScoreAsync(newScore, CancellationToken.None);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _timer.DisposeAsync();
    }
}