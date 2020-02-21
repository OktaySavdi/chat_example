using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Models
{
    public class ElasticLogItem
    {
        [Text(Name = "level")]
        public string Level { get; set; }
        
        [Text(Name = "message")]
        public string Message { get; set; }
                    

    }
}
