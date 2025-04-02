using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace MongoDataAccess;

public static class MongoDbAccessExt
{
    public static void AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDb");
        var mongoClient = new MongoClient(connectionString);
        var database = mongoClient.GetDatabase("leaderboards");
        services.AddSingleton<IMongoClient>(mongoClient);
        services.AddSingleton<IMongoDatabase>(database);
        services.AddScoped<IPlayerScoreRepository, PlayerScoreRepository>();
    }
}