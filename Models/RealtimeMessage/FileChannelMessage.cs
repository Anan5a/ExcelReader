namespace Models.RealtimeMessage
{
    public class FileChannelMessage
    {
        public string Message { get; set; } = "A file event occurred";
        public bool WasFileDeleted { get; set; } = false;
        public bool WasFileModified { get; set; } = false;
        public bool ShouldRefetch { get; set; } = false;
        public long FileId { get; set; } = 0;
        public FileMetadata? FileMetadata { get; set; } = null;
        public FileChannelMessage() { }
    }
}
