using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExcelReader.Models
{
    public class User
    {
        [Key]
        [BindNever]
        public long Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [JsonIgnore]
        public string Password { get; set; }

        [Required]
        [BindNever]
        public DateTime CreatedAt { get; set; }
        
        [BindNever]
        public DateTime? UpdatedAt { get; set; }

        [BindNever]
        
        public DateTime? DeletedAt { get; set; }

        [Required]
        public long RoleId { get; set; }

        [BindNever]
        [JsonIgnore]
        public string? PasswordResetId { get; set; }
        public DateTime? VerifiedAt { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression("^(ok|pending|disabled)$", ErrorMessage = "Invalid status value.")]
        public string Status { get; set; }

        [BindNever]
        public Role? Role { get; set; }

        // Default constructor
        public User() { }

        // Parameterized constructor
        public User(long id, string name, string email, string password, DateTime createdAt,
                    DateTime? updatedAt, DateTime? deletedAt, long roleId, string passwordResetId,
                    DateTime? verifiedAt, string status, Role? role)
        {
            Id = id;
            Name = name;
            Email = email;
            Password = password;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            DeletedAt = deletedAt;
            RoleId = roleId;
            PasswordResetId = passwordResetId;
            VerifiedAt = verifiedAt;
            Status = status;
            Role = role;
        }
    }

}
