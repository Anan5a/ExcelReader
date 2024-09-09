using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class UserSignUpDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        // Default constructor
        public UserSignUpDTO() { }

        // Parameterized constructor
        public UserSignUpDTO(string name, string email, string password)
        {
            Name = name;
            Email = email;
            Password = password;
        }
    }
}
