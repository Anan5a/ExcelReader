using System.ComponentModel.DataAnnotations;

namespace ExcelReader.Models
{
    public class Role
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression("^(super_admin|admin|user|guest)$", ErrorMessage = "Invalid role name.")]
        public string RoleName { get; set; }

        // Default constructor
        public Role() { }

        // Parameterized constructor
        public Role(long id, string roleName)
        {
            Id = id;
            RoleName = roleName;
        }
    }

}
