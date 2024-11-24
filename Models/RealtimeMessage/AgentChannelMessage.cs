namespace Models.RealtimeMessage
{
    public class AgentChannelMessage<MetaT>
    {
        public string Message { get; set; } = "A new agent event";
        public string? CallData { get; set; }
        public MetaT? Metadata { get; set; }
        public bool? IsSystemMessage { get; set; }

    }
}
