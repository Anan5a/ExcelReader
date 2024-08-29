
namespace DataAccess.IRepository
{
    public interface IRepository<T> where T : class
    {
        IMyDbConnection dbConnection { get; set; }
        ulong Add(T entity);
        IEnumerable<T>? GetAll(Dictionary<string, dynamic>? condition = null, bool resolveRelation=false);
        T? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false);
        T? Update(T entity);
        int Remove(string TableName, string ColumnName, int Id);
        int RemoveRange(string TableName, string ColumnName, List<int> Ids);
    }
}
