using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ExcelReader.Models
{
    public class User
    {
        [BindNever]
        [ValidateNever]
        public long Id { get; set; }
        [Required]
        public string UUID { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [ValidateNever]
        [BindNever]
        public DateTime CreatedAt { get; set; }
        public User(string uuid, string name, string email, DateTime createdAt)
        {
            UUID = uuid;
            Name = name;
            Email = email;
            CreatedAt = createdAt;
        }
        public User() { }   
    }
}
