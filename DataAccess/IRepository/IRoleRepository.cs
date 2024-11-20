
using Models;

namespace IRepository
{
    public interface IRoleRepository : IRepository<Role>
    {

        new long Add(Role user);
        new IEnumerable<Role> GetAll(Dictionary<string, dynamic>? condition, bool resolveRelation = false, bool lastOnly = true, int n = 10, bool whereConditionUseOR = false);
        new Role? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false);
        new Role? Update(Role user);
        int Remove(int id);
        int RemoveRange(List<int> Ids);
        long Count(Dictionary<string, dynamic>? condition = null);

    }
}


