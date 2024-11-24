using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Models
{
    public class RTCConnModel
    {
        [Required]
        public int? TargetUserId { get; set; }
        [AllowNull]
        public string? TargetUserName { get; set; }
        [Required]
        public string? Data { get; set; }
        
    }
}
