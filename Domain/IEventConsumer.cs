namespace Domain;

public interface IEventConsumer
{
    Task<List<ScoresAdded>> GetNewScoresAsync(CancellationToken stoppingToken);
}