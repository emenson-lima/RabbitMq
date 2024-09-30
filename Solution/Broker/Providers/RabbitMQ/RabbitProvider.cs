using System.Net.Sockets;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Broker.Providers.RabbitMQ;

public class RabbitProvider
{
    private IModel _channel;
    private IConnection _connection;
    private IConnectionFactory _factory;
    private readonly string _appName = AppDomain.CurrentDomain.FriendlyName;
    private readonly string _machineName = Environment.MachineName;
    
    public IModel CreateChannel()
    {
        try
        {
            CreateConnection();
            
            if (_channel != null || _channel.IsOpen) return _channel;
            
            _channel = _connection.CreateModel();
            
            _channel.ConfirmSelect();
            
            var channelNumber = _channel.ChannelNumber;
            
            _channel.ModelShutdown += HandleShutdownEvent;
            _channel.CallbackException += HandleCallbackException;
            _channel.BasicReturn += HandleBasicReturn;

            return _channel;
        }
        catch (BrokerUnreachableException ex)
        {
            // ignore
        }
        catch (OperationInterruptedException ex)
        {
            // ignore
        }
        catch (IOException ex)
        {
            // ignore
        }
        catch (Exception ex)
        {
            // ignore
        }
        
        return _channel;
    }
    
    private void CreateConnection()
    {
        try
        {
            if (_factory != null) return;

            if (_connection != null || _connection.IsOpen) return;

            _factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/",
                Port = AmqpTcpEndpoint.UseDefaultPort,
                ClientProvidedName = $"App: {_appName}, Machine: {_machineName}",
                RequestedConnectionTimeout = TimeSpan.FromMilliseconds(1000),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromMilliseconds(1000),
            };

            _connection = _factory.CreateConnection();
            
            _connection.ConnectionShutdown += HandleShutdownEvent;
            _connection.CallbackException += HandleCallbackException;
            _connection.ConnectionBlocked += HandleBlockedEvent;
        }
        catch (BrokerUnreachableException ex)
        {
            // ignore
        }
        catch (OperationInterruptedException ex)
        {
            // ignore
        }
        catch (SocketException ex)
        {
            // ignore
        }
        catch (Exception exception)
        {
            // ignore
        }
    }
    
    private void HandleShutdownEvent(object sender, ShutdownEventArgs e)
    {
        switch (sender)
        {
            case IConnection:
                break;
            case IModel:
                break;
        }
    }

    private void HandleCallbackException(object sender, CallbackExceptionEventArgs e)
    {
        switch (sender)
        {
            case IConnection:
                break;
            case IModel:
                break;
        }
    }

    private void HandleBlockedEvent(object sender, ConnectionBlockedEventArgs e)
    {
    }

    private void HandleBasicReturn(object sender, BasicReturnEventArgs e)
    {
    }
}