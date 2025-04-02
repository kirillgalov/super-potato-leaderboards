namespace Domain;

public interface IEventProducer
{
    Task PublishNewScoreAsync(ScoresAdded scoresAdded, CancellationToken token);
}