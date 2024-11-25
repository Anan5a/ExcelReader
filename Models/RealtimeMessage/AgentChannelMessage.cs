using System.Text.Json.Serialization;

namespace Models.RealtimeMessage
{

    public class AgentChannelMessage<MetaT>
    {
        public string Message { get; set; } = "A new agent event";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CallData { get; set; }

        public MetaT? Metadata { get; set; }
        public bool? IsSystemMessage { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ContainsUser { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ContainsCallData { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AcceptIntoChat { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? RemoveUserFromList { get; set; }

    }
}
