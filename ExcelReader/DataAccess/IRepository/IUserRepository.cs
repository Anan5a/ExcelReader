using ExcelReader.Models;

namespace DataAccess.IRepository
{
    public interface IUserRepository : IRepository<User>
    {
        new int Add(User user);
        new IEnumerable<User> GetAll(Dictionary<string, dynamic>? condition);
        new User? Get(Dictionary<string, dynamic> condition);
        new User? Update(User user);
        int Remove(int id);
        int RemoveRange(List<int> Ids);
    }
}
