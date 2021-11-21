using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogApi.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LogApi.Services
{
    public class Service : IService
    {
        private const string QueueName = "logsQueue";
        public static List<string> messages;
        private IModel channel;
        public Service() 
        {
            messages = new List<string>();
            var factory = new ConnectionFactory { HostName = "localhost" };
            IConnection connection = factory.CreateConnection();
            channel = connection.CreateModel();

            DeclareQueue(channel);
            ReceiveMessages(channel);
        }

        public void DeclareQueue(IModel channel)
        {
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
                messages.Add(Encoding.UTF8.GetString(body));
            };

            channel.BasicConsume(
                queue: QueueName,
                autoAck: true,
                consumer: consumer);
        }

        public Task<List<string>> GetMessages(string user, string game, string date)
        {
            
            ReceiveMessages(channel);
            List<string> filteredList = new List<string>();
            if (user != null)
            {
                foreach (var line in messages)
                {
                    if (line.Contains(user)){
                        filteredList.Add(line);
                    }
                }
            }
            if (game != null)
            {
                
                if (filteredList.Count > 0)
                {
                    for (int i = 0; i < filteredList.Count(); i++)
                    {
                        if (!filteredList[i].Contains(game))
                        {
                            filteredList.Remove(filteredList[i]);
                        }
                    }
                }
                else
                {
                    foreach (var line in messages)
                    {
                        if (line.Contains(game))
                        {
                            filteredList.Add(line);
                        }
                    }
                }
            }
            if (date !=null)
            {
                if (filteredList.Count > 0)
                {
                    for (int i = 0; i < filteredList.Count(); i++)
                    {
                        if (!filteredList[i].Contains(date))
                        {
                            filteredList.Remove(filteredList[i]);
                        }
                    }
                }
                else
                {
                    foreach (var line in messages)
                    {
                        if (line.Contains(date))
                        {
                            filteredList.Add(line);
                        }
                    }
                }
            }

            return filteredList.Count() > 0 ? Task.FromResult(filteredList) : Task.FromResult(messages);
        }
    }
}
