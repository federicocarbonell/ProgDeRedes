using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace LogServer
{
    class Program
    {
        private const string QueueName = "logsQueue";

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using IConnection connection = factory.CreateConnection();
            using IModel channel = connection.CreateModel();

            DeclareQueue(channel);
            ReceiveMessages(channel);

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void DeclareQueue(IModel channel)
        {
            //Name (queue name)
            //Durable(the queue will survive a broker restart)
            //Exclusive(used by only one connection and the queue will be deleted when that connection closes)
            //Auto - delete(queue that has had at least one consumer is deleted when last consumer unsubscribes)
            //Arguments(optional; used by plugins and broker - specific features such as message TTL, queue length limit, etc)

            channel.QueueDeclare(
                queue: QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        private static void ReceiveMessages(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Received message : " + message);
            };

            channel.BasicConsume(
                queue: QueueName,
                autoAck: true,
                consumer: consumer);
        }
    }
}
