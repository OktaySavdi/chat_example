using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Models
{
    public class ChatMessage
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
        
    }
}
