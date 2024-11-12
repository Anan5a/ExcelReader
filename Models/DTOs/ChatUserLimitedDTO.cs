using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class ChatUserLimitedDTO
    {
        [Key]
        [BindNever]
        public long Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [BindNever]
        public ChatUserLimitedDTO? AgentInfo { get; set; }
    }
}
