using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class GroupModel
    {
        public long GroupId { get; set; }
        public required string GroupName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public List<GroupMembershipModel>? Members { get; set; }
    }
}