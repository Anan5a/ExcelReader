using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class CreateGroupMembershipDTO
    {
        /// <summary>
        /// The ID of the group associated with the membership.
        /// </summary>
        [Required(ErrorMessage = "GroupId is required.")]
        [Range(1, long.MaxValue, ErrorMessage = "GroupId must be a valid positive number.")]
        public long GroupId { get; set; }

        /// <summary>
        /// The ID of the user associated with the membership.
        /// </summary>
        [Required(ErrorMessage = "UserId is required.")]
        [Range(1, long.MaxValue, ErrorMessage = "UserId must be a valid positive number.")]
        public long UserId { get; set; }
    }
}
