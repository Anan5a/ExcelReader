﻿namespace Models.RealtimeMessage
{
    public class ChatChannelMessage
    {
        public long? MessageId { get; set; }
        public string Message { get; set; } = "A new message is received";

        public long From { get; set; } = 0;
        public string Content { get; set; } = "";
        public bool? IsSystemMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public ChatChannelMessage() { }
    }
}
