using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Models
{
    public class ChatHistory
    {
        [BindNever]
        public long ChatHistoryId { get; set; }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public string Content { get; set; }
        [BindNever]
        public DateTime CreatedAt { get; set; }
    }
}