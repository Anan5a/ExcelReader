using System.ComponentModel.DataAnnotations;



namespace Models.DTOs
{
    public class UserLoginDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        // Default constructor
        public UserLoginDTO() { }

        // Parameterized constructor
        public UserLoginDTO(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}