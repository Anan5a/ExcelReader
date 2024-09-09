using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class AdminNewUserDTO
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

        [Required(ErrorMessage = "Role id is required.")]
        public long RoleId { get; set; }


        // Default constructor
        public AdminNewUserDTO() { }

        // Parameterized constructor
        public AdminNewUserDTO(string name, string email, string password, long roleId)
        {
            Name = name;
            Email = email;
            Password = password;
            RoleId = roleId;
        }
    }
}
