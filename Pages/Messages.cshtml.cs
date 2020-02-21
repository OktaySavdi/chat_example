using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Web.Models;
using Chat.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chat.Web
{
    public class MessagesModel : PageModel
    {
        private readonly RabbitMQService rabbitMQService;

        public List<ChatMessage> Messages { get; set; }

        public MessagesModel(RabbitMQService rabbitMQService)
        {
            this.rabbitMQService = rabbitMQService;
        }

        public IActionResult OnGet()
        {
            Messages = rabbitMQService.GetAll();

            return Page();
        }


        public IActionResult OnPostDeleteMessages()
        {
            rabbitMQService.DeleteAllMessages();
            return new RedirectToPageResult("Index");
        }

    }
}