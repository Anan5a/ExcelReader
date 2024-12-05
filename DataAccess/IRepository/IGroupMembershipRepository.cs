using IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepository
{
    public interface IGroupMembershipRepository : IRepository<GroupMembershipModel>
    {
        new long Add(GroupMembershipModel groupMembership);
        new IEnumerable<GroupMembershipModel> GetAll(Dictionary<string, dynamic>? condition, bool resolveRelation = false, bool lastOnly = true, int n = 10, bool whereConditionUseOR = false);
        new GroupMembershipModel? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false);
        new GroupMembershipModel? Update(GroupMembershipModel user);
        int Remove(int id);
        int RemoveRange(List<int> Ids);
        long Count(Dictionary<string, dynamic>? condition = null);
    }
}