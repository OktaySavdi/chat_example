using Chat.Web.Models;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chat.Web.Services
{
    public class RabbitMQService
    {
        private readonly IConfiguration configuration;
        private const string EXCHANGE_NAME = "Chat_Exchange";
        private const string QUEUE_NAME = "Chats";
        private const string ROUTING_KEY = "ChatMessage";
        public RabbitMQService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        ///<summary>
        ///Adds a message to specific queue
        /// </summary>
        public void AddData(ChatMessage chatMessage)
        {

            var factory = new ConnectionFactory() { HostName = configuration["RabbitMQ"] };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                string message = JsonSerializer.Serialize(chatMessage);
                var body = Encoding.UTF8.GetBytes(message);

                channel.ExchangeDeclare(EXCHANGE_NAME, ExchangeType.Direct, true, false, null);
                channel.QueueDeclare(QUEUE_NAME, true, false, false, null);
                channel.QueueBind(QUEUE_NAME, EXCHANGE_NAME, ROUTING_KEY, null);
                channel.BasicPublish(EXCHANGE_NAME,
                                                   routingKey: ROUTING_KEY,
                                                   basicProperties: null,
                                                   body: body);

            }
        }

        ///<summary>
        ///Retrieves all messages from queue
        /// </summary>
        public List<ChatMessage> GetAll()
        {
            List<ChatMessage> messageList = new List<ChatMessage>();

            var factory = new ConnectionFactory() { HostName = configuration["RabbitMQ"] };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
              
                var consumer = new EventingBasicConsumer(channel);


                while (true)
                {
                   
                    var data = channel.BasicGet(QUEUE_NAME, false);
                    if (data == null)
                    {

                        break;
                    }

                    var message = Encoding.UTF8.GetString(data.Body);
                    ChatMessage chatMessage = JsonSerializer.Deserialize<ChatMessage>(message);
                    messageList.Add(chatMessage);

                }

                return messageList;

            }


        }

        ///<summary>
        ///Retrieves "Received Messages" of specific chat user
        /// </summary>
        public List<ChatMessage> GetReceivedMessageList(string ToId)
        {
            List<ChatMessage> messageList = new List<ChatMessage>();

            var factory = new ConnectionFactory() { HostName = configuration["RabbitMQ"] };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var consumer = new EventingBasicConsumer(channel);


            while (true)
            {

                var data = channel.BasicGet(QUEUE_NAME, false);
                if (data == null)
                {
                    break;
                }

                var message = Encoding.UTF8.GetString(data.Body);
                ChatMessage chatMessage = JsonSerializer.Deserialize<ChatMessage>(message);
                if (chatMessage.To == ToId)
                    messageList.Add(chatMessage);

            }

            return messageList;


        }


        ///<summary>
        ///Retrieves "Sent Messages" of specific chat user
        /// </summary>
        public List<ChatMessage> GetSentMessageList(string FromId)
        {
            List<ChatMessage> messageList = new List<ChatMessage>();

            var factory = new ConnectionFactory() { HostName = configuration["RabbitMQ"] };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var consumer = new EventingBasicConsumer(channel);


            while (true)
            {

                var data = channel.BasicGet(QUEUE_NAME, false);
                if (data == null)
                {
                    break;
                }

                var message = Encoding.UTF8.GetString(data.Body);
                ChatMessage chatMessage = JsonSerializer.Deserialize<ChatMessage>(message);
                if (chatMessage.From == FromId)
                    messageList.Add(chatMessage);

            }

            return messageList;


        }

        ///<summary>
        ///Removes all existing messages and purges the queue
        /// </summary>
        public void DeleteAllMessages()
        {
            var factory = new ConnectionFactory() { HostName = configuration["RabbitMQ"] };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {

                var consumer = new EventingBasicConsumer(channel);
                channel.QueuePurge(QUEUE_NAME);

            }


        }

    }

}
