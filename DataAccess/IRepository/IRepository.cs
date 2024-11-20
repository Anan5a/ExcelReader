
using DataAccess;

namespace IRepository
{
    public interface IRepository<T> where T : class
    {
        IMyDbConnection dbConnection { get; set; }
        long Add(T entity);
        long Count(string TableName, Dictionary<string, dynamic>? condition = null);

        IEnumerable<T>? GetAll(Dictionary<string, dynamic>? condition = null, bool resolveRelation = false, bool lastOnly = true, int n = 10, bool whereConditionUseOR = false);

        T? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false);
        T? Update(T entity);
        int Remove(string TableName, string ColumnName, int Id);
        int RemoveRange(string TableName, string ColumnName, List<int> Ids);

    }
}
