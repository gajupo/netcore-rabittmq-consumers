namespace rabbit_consumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IRabbitMqSubscriber _rabbitMqSubscriber;


    public Worker(ILogger<Worker> logger, IRabbitMqSubscriber rabbitMqSubscriber)
    {
        _logger = logger;
        _rabbitMqSubscriber = rabbitMqSubscriber;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

       _logger.LogInformation("RabbitMQ subscriber started.");
       await _rabbitMqSubscriber.Subscribe(stoppingToken);
    }
}