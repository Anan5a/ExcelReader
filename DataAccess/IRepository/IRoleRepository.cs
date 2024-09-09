
using Models;

namespace IRepository
{
    public interface IRoleRepository : IRepository<Role>
    {

        new ulong Add(Role user);
        new IEnumerable<Role> GetAll(Dictionary<string, dynamic>? condition, bool resolveRelation = false);
        new Role? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false);
        new Role? Update(Role user);
        int Remove(int id);
        int RemoveRange(List<int> Ids);
        ulong Count(Dictionary<string, dynamic>? condition = null);

    }
}


