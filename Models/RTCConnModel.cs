using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class RTCConnModel
    {
        [Required]
        public int TargetUserId { get; set; }
        [Required]
        public string Data { get; set; }

    }
}
