using rabbit_consumer;
using rabbit_consumer.services;
using rabbit_consumer.services.core;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IRabbitMqSubscriber, RabbitMqSubscriber>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
