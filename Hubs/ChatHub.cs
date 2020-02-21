using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chat.Web.Models;
using Chat.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chat.Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly RabbitMQService rabbitMQService;
        private readonly RedisService chatConnectionService;

        public ChatHub(ILogger<ChatHub> logger, RabbitMQService rabbitMQService, RedisService chatConnectionService)
        {
            _logger = logger;
            this.rabbitMQService = rabbitMQService;
            this.chatConnectionService = chatConnectionService;
        }


        ///<summary>
        ///Sends message to all connected chat users
        /// </summary>
        public Task SendMessageToAll(string message)
        {

            Task<List<string>> connecionIdList = chatConnectionService.GetConnectionIdList();

            foreach (var connectionId in connecionIdList.Result)
            {

                if (Context.ConnectionId == connectionId) continue;
                
                //store data on rabbitmq
                ChatMessage chatMessage = new ChatMessage
                {
                    To = connectionId,
                    From = Context.ConnectionId,
                    Message = message,
                    Created = DateTime.Now
                };
                rabbitMQService.AddData(chatMessage);
            }

            return  Clients.All.SendAsync("ReceiveMessage", message);
        }



        ///<summary>
        ///Sends message to specific chat user
        /// </summary>
        public Task SendMessageToUser(string connectionId, string message)
        {
            var info = new { senderId = Context.ConnectionId, receiverId = connectionId, message };
            _logger.LogInformation("Chat Activity: {@info} ", info);

            //store data on rabbitmq
            ChatMessage chatMessage = new ChatMessage
            {
                To = connectionId,
                From = Context.ConnectionId,
                Message = message,
                Created = DateTime.Now
            };
            rabbitMQService.AddData(chatMessage);

            return Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
        }

        ///<summary>
        ///Occurs immediately when a user is connected
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await chatConnectionService.AddConnectionId(Context.ConnectionId);
            await Clients.All.SendAsync("UserConnected", String.Join(",", await chatConnectionService.GetConnectionIdList()));
            await base.OnConnectedAsync();
        }

        ///<summary>
        ///Occurs immediately when a user is disconnected
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await chatConnectionService.RemoveConnectionId(Context.ConnectionId);
            await Clients.All.SendAsync("UserConnected", String.Join(",", await chatConnectionService.GetConnectionIdList()));
            await base.OnDisconnectedAsync(ex);
        }

        #region ThrowHubException
        public Task ThrowException()
        {
            throw new HubException("This error will be sent to the client!");
        }
        #endregion
    }
}