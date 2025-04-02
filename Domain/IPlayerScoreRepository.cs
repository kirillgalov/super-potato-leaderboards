namespace Domain;

public interface IPlayerScoreRepository
{
    Task<PlayerScore?> GetPlayerScoreAsync(int userId, CancellationToken token);
    Task<List<PlayerScore>> GetPlayerScoresAsync(int offset, int limit, CancellationToken token);
    Task UpdateScoresAsync(IEnumerable<ScoresAdded> newScores, CancellationToken token);

}