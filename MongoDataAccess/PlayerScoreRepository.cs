using Domain;
using MongoDB.Driver;

namespace MongoDataAccess;

public class PlayerScoreRepository : IPlayerScoreRepository
{
    private readonly IMongoCollection<PlayerScore> _playerScores;

    public PlayerScoreRepository(IMongoDatabase database)
    {
        _playerScores = database.GetCollection<PlayerScore>("playerScores");
    }

    public async Task<PlayerScore?> GetPlayerScoreAsync(int userId, CancellationToken token)
    {
        PlayerScore? playerScore = await _playerScores.Find(playerScore => playerScore.UserId == userId).FirstOrDefaultAsync(token);

        if (playerScore == null)
        {
            return null;
        }

        long count  = await _playerScores.Find(ps => ps.TotalScore > playerScore.TotalScore ||
                                                     ps.TotalScore == playerScore.TotalScore && ps.UpdatedAt < playerScore.UpdatedAt).CountDocumentsAsync(token);
        playerScore.Place = count;
        return playerScore;
    }

    public async Task<List<PlayerScore>> GetPlayerScoresAsync(int offset, int limit, CancellationToken token)
    {
        var playerScores = await _playerScores.Find(_ => true)
            .Sort(Builders<PlayerScore>.Sort.Descending(ps => ps.TotalScore))
            .Skip(offset)
            .Limit(limit)
            .ToListAsync(token);

        int place = offset + 1;
        foreach (var playerScore in playerScores)
        {
            playerScore.Place = place++;
        }

        return playerScores;
    }

    public async Task UpdateScoresAsync(IEnumerable<ScoresAdded> newScores, CancellationToken token)
    {
        IEnumerable<WriteModel<PlayerScore>> writes = newScores.Select(sa =>
        {
            FilterDefinition<PlayerScore> filter = Builders<PlayerScore>.Filter.Eq(ps => ps.UserId, sa.UserId);
            UpdateDefinition<PlayerScore> update = Builders<PlayerScore>.Update.Inc(ps => ps.TotalScore, sa.Score).Set(ps => ps.UpdatedAt, DateTime.UtcNow);
            WriteModel<PlayerScore> writeModel = new UpdateOneModel<PlayerScore>(filter, update){ IsUpsert = true , };
            return writeModel;
        });

        await _playerScores.BulkWriteAsync(writes, cancellationToken: token);
    }
}