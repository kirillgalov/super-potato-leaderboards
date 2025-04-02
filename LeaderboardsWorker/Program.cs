using LeaderboardsWorker.BackgroundServices;
using MongoDataAccess;
using RabbitMQAccess;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddRabbitMQConsumer();
builder.Services.Configure<WorkerSettings>(builder.Configuration.GetSection("WorkerSettings"));
builder.Services.AddHostedService<ScoresWorker>();

var app = builder.Build();

app.MapControllers();

app.Run();