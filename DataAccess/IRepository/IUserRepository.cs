﻿
using Models;

namespace IRepository
{
    public interface IUserRepository : IRepository<User>
    {
        new long Add(User user);
        new IEnumerable<User> GetAll(Dictionary<string, dynamic>? condition, bool resolveRelation = false);
        new User? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false);
        new User? Update(User user);
        int Remove(int id);
        int RemoveRange(List<int> Ids);
        long Count(Dictionary<string, dynamic>? condition = null);

    }
}
