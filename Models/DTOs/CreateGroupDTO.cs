using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class CreateGroupDTO
    {
        /// <summary>
        /// The name of the group to be created.
        /// </summary>
        [Required(ErrorMessage = "Group name is required.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Group name must be between 4 and 100 characters.")]
        public required string GroupName { get; set; }

        ///// <summary>
        ///// A description of the group.
        ///// </summary>
        //[StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        //public string? Description { get; set; }

        ///// <summary>
        ///// The ID of the user creating the group (e.g., the admin or owner).
        ///// </summary>
        //[Required(ErrorMessage = "CreatedBy (user ID) is required.")]
        //[Range(1, long.MaxValue, ErrorMessage = "CreatedBy must be a valid user ID.")]
        //public long CreatedBy { get; set; }
    }
}
