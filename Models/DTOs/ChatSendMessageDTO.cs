using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class ChatSendMessageDTO
    {
        [Required(ErrorMessage = "Message is required.")]
        [MaxLength(255, ErrorMessage = "Message cannot exceed 255 characters.")]
        public string Message { get; set; }

        [Required(ErrorMessage = "Recipient is required.")]
        public long To { get; set; }

        // Default constructor
        public ChatSendMessageDTO() { }

        // Parameterized constructor
        public ChatSendMessageDTO(long to, string message)
        {
            Message = message;
            To = to;
        }
    }
}
