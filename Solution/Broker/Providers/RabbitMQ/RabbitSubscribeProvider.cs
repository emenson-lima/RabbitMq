using System.Text;
using Broker.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Broker.Providers.RabbitMQ;

public class RabbitSubscribeProvider
{
    private IModel _channel;
    private IBasicProperties _properties;
    private IEventHandler _eventHandler;

    public void Subscribe(string queueName, string exchange, string exchangetype, string routingKey)
    {
        try
        {
            var provider = new RabbitProvider();

            _channel = provider.CreateChannel();

            _channel.ExchangeDeclare(
                exchange: exchange,
                type: exchangetype,
                durable: true,
                autoDelete: false,
                arguments: null
            );
            
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            
            _channel.QueueBind(queue: queueName, exchange: exchange, routingKey: routingKey);
            
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var messageId = ea.BasicProperties.MessageId;
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    _eventHandler.Handle(message);
                    
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception e)
                {
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true); 
                }
            };
            
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }
        catch (Exception ex)
        {
            // ignore
        }
    }
}