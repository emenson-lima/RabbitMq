namespace Broker.Interfaces;

public interface IEventHandler
{
    void Handle(string message);
}