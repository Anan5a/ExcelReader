using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class CallQueueModel
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public string CallId { get; set; }
        public DateTime CallTime { get; set; }

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }

}
