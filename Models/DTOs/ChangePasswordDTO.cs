using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Old Password is required.")]
        [MinLength(8, ErrorMessage = "Old Password must be at least 8 characters long.")]
        public string oldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [MinLength(8, ErrorMessage = "New Password must be at least 8 characters long.")]
        public string newPassword { get; set; }

    }
}
