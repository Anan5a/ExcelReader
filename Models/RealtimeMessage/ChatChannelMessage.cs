namespace Models.RealtimeMessage
{
    public class ChatChannelMessage
    {
        public string Message { get; set; } = "A new message is received";

        public long From { get; set; } = 0;
        public string Content { get; set; } = "";
        public ChatChannelMessage() { }
    }
}
