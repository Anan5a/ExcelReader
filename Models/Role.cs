using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Role
    {
        [BindNever]
        public long RoleId { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression("^(super_admin|admin|user|guest)$", ErrorMessage = "Invalid role name.")]
        public string RoleName { get; set; }

        // Default constructor
        public Role() { }

        // Parameterized constructor
        public Role(long id, string roleName)
        {
            RoleId = id;
            RoleName = roleName;
        }
    }

}