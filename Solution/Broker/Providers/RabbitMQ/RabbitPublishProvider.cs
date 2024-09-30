using System.Text;
using RabbitMQ.Client;

namespace Broker.Providers.RabbitMQ;

public class RabbitPublishProvider
{
    private IModel _channel;
    private IBasicProperties _properties;

    public void Publish(string message, string exchange, string exchangetype, string routingKey)
    {
        try
        {
            var messageId =  Guid.NewGuid().ToString();
            
            var provider = new RabbitProvider();

            _channel = provider.CreateChannel();

            _channel.ExchangeDeclare(
                exchange: exchange,
                type: exchangetype,
                durable: true,
                autoDelete: false,
                arguments: null
            );
            
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; 
            properties.ContentType = "application/json"; 
            properties.Priority = 0; 
            properties.MessageId = messageId;
            properties.DeliveryMode = 2;

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: Encoding.UTF8.GetBytes(message)
            );

            var confirmReceived = _channel.WaitForConfirms(TimeSpan.FromMilliseconds(250));

            if (!confirmReceived)
                throw new Exception("RabbitMQ channel confirmation failed");
        }
        catch (Exception ex)
        {
            // ignore
        }
    }
}