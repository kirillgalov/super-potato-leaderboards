using GameServer.BackgroundServices;
using RabbitMQAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<MockGameService>();
builder.Services.AddRabbitMQProducer();

var app = builder.Build();

app.Run();