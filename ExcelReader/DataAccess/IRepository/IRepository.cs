
namespace DataAccess.IRepository
{
    public interface IRepository<T> where T : class
    {
        IMyDbConnection dbConnection { get; set; }
        int Add(T entity);
        IEnumerable<T>? GetAll(Dictionary<string, dynamic>? condition = null);
        T? Get(Dictionary<string, dynamic> condition);
        T? Update(T entity);
        int Remove(string TableName, string ColumnName, int Id);
        int RemoveRange(string TableName, string ColumnName, List<int> Ids);
    }
}
