using IRepository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepository
{
    public interface IGroupRepository : IRepository<GroupModel>
    {

        new long Add(GroupModel group);
        new IEnumerable<GroupModel> GetAll(Dictionary<string, dynamic>? condition, bool resolveRelation = false, bool lastOnly = true, int n = 10, bool whereConditionUseOR = false);
        new GroupModel? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false);
        new GroupModel? Update(GroupModel user);
        int Remove(int id);
        int RemoveRange(List<int> Ids);
        long Count(Dictionary<string, dynamic>? condition = null);
        IEnumerable<GroupModel> GetGroupsWithMembersAndDetails(Dictionary<string, dynamic>? condition = null, bool whereConditionUseOR = false);
    }
}