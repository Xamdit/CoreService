using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Service.Framework.Library.Net;

public class RabbitMqLinq : IDisposable
{
  private readonly IConnection _connection;
  private readonly IModel _channel;
  private readonly string _queueName;
  private readonly List<string> _messages;

  public RabbitMqLinq(string hostName, string queueName)
  {
    var factory = new ConnectionFactory { HostName = hostName };
    _connection = factory.CreateConnection();
    _channel = _connection.CreateModel();
    _queueName = queueName;

    _channel.QueueDeclare(queueName, false, false, false, null);
    _messages = new List<string>();
    LoadMessages();
  }

  private void LoadMessages()
  {
    var consumer = new EventingBasicConsumer(_channel);
    consumer.Received += (model, ea) =>
    {
      var body = ea.Body.ToArray();
      var message = Encoding.UTF8.GetString(body);
      _messages.Add(message);
    };
    _channel.BasicConsume(_queueName, true, consumer);
  }

  public IEnumerable<string> Where(Func<string, bool> predicate)
  {
    return _messages.Where(predicate);
  }

  public IEnumerable<TResult> Select<TResult>(Func<string, TResult> selector)
  {
    return _messages.Select(selector);
  }

  public void Publish(string message)
  {
    var body = Encoding.UTF8.GetBytes(message);
    _channel.BasicPublish("", _queueName, null, body);
  }

  public void Dispose()
  {
    _channel?.Close();
    _connection?.Close();
  }

  [Test]
  public void Test()
  {
    using var rabbitMq = new RabbitMqLinq("localhost", "testQueue");
    // Publish some messages
    rabbitMq.Publish("Hello, RabbitMQ!");
    rabbitMq.Publish("Another message");
    rabbitMq.Publish("LINQ with RabbitMQ");

    // Query messages using LINQ
    var filteredMessages = rabbitMq.Where(msg => msg.Contains("RabbitMQ"));
    foreach (var message in filteredMessages) Console.WriteLine($"Filtered Message: {message}");

    // Transform messages using Select
    var transformedMessages = rabbitMq.Select(msg => msg.ToUpper());
    foreach (var message in transformedMessages) Console.WriteLine($"Transformed Message: {message}");
  }
}
