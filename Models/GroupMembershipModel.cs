using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class GroupMembershipModel
    {
        public long GroupMembershipId { get; set; }
        public long GroupId { get; set; }
        public long UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public User? UserDetails { get; set; }
    }
}