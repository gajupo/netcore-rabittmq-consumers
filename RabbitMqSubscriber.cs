using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rabbit_consumer.services.core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace rabbit_consumer
{
    public class RabbitMqSubscriber : IRabbitMqSubscriber, IDisposable
    {
        private readonly ILogger<RabbitMqSubscriber> _logger;
        private readonly IConfiguration _configuration;
        private readonly string? _exchangeName = string.Empty;
        private readonly string? _queueName = string.Empty;
        private readonly IModel _model;
        private readonly IConnection _connection;

        private readonly SemaphoreSlim semaphoreSlim;

        public RabbitMqSubscriber(ILogger<RabbitMqSubscriber> logger, IConfiguration configuration, IRabbitMqService rabbitMqService)
        {
            _logger = logger;
            _configuration = configuration;
            _exchangeName = _configuration.GetValue<string>("RabbitConsumer:ExchangeName");
            _queueName = _configuration.GetValue<string>("RabbitConsumer:QueueName");

            // set the max message the service can process concurrently
            int maxConcurrentMessages = _configuration.GetValue<int>("RabbitConsumer:MaxConcurrentMessages");
            semaphoreSlim = new SemaphoreSlim(maxConcurrentMessages, maxConcurrentMessages);

            _connection = rabbitMqService.CreateChannel();
            _model = _connection.CreateModel();
            _model.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false);
            _model.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _model.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: "");
        }

        public async Task Subscribe(CancellationToken cancellationToken)
        {

            _model.BasicQos(0, (ushort)semaphoreSlim.CurrentCount, false);

            var consumer = new AsyncEventingBasicConsumer(_model);

            consumer.Received += async (model, ea) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                await semaphoreSlim.WaitAsync(cancellationToken);
                _ = ProcessMessage(ea, cancellationToken);
            };

            _model.BasicConsume(_queueName, false, consumer);

            await Task.CompletedTask;
        }

        private async Task ProcessMessage(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received message: {message} at {DateTime.UtcNow}");

                await Task.Delay(5000,cancellationToken);

                _model.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message: {ex.Message}");
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing RabbitMQSubscriber..");
            if (_model.IsOpen)
                _model.Close();
            if (_connection.IsOpen)
                _connection.Close();

            semaphoreSlim.Dispose();
        }
    }
}