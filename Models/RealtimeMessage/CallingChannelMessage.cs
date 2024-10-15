namespace Models.RealtimeMessage
{
    public class CallingChannelMessage
    {
        public string Message { get; set; } = "A new call event";
        public string CallData { get; set; }
        public RTCConnModel Metadata { get; set; }
    }
}
