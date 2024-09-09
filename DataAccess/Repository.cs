using IRepository;
using Microsoft.Data.SqlClient;
using Services;
using System.Text;

namespace DataAccess
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public IMyDbConnection dbConnection { get; set; }

        public Repository(IMyDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public int Remove(string TableName, string ColumnName, int Id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();
                    string sql = $"DELETE FROM {TableName} WHERE {ColumnName} = @Id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", Id);
                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
                return 0;
            }

        }

        public int RemoveRange(string TableName, string ColumnName, List<int> Ids)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();
                    string sql = $"DELETE FROM {TableName} WHERE {ColumnName} IN ({string.Join(",", Ids)})";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
                return 0;
            }
        }

        public ulong Add(T entity)
        {
            throw new NotImplementedException("Must implement specific logic in the repository");
        }

        public IEnumerable<T> GetAll(Dictionary<string, dynamic>? condition = null, bool resolveRelation = false)
        {
            throw new NotImplementedException("Must implement specific logic in the repository");
        }

        public T? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false)
        {
            throw new NotImplementedException("Must implement specific logic in the repository");
        }

        public T Update(T entity)
        {
            throw new NotImplementedException("Must implement specific logic in the repository");
        }

        public ulong Count(string tableName, Dictionary<string, dynamic>? condition = null)
        {
            ulong rowCount = 0;
            StringBuilder whereClause = new StringBuilder();
            List<SqlParameter> parameters = new List<SqlParameter>();

            if (condition != null)
            {

                foreach (var pair in condition)
                {
                    if (whereClause.Length > 0)
                        whereClause.Append(" AND ");

                    whereClause.Append($"[{pair.Key}] = @{pair.Key}");
                    parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                }

            }
            string query = $@"
                SELECT COUNT(*)
                FROM [{tableName}]";
            if (whereClause.Length > 0)
            {
                query += $" WHERE {whereClause}";
            }

            using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    rowCount = Convert.ToUInt64(command.ExecuteScalar());
                }
            }

            return rowCount;
        }

    }
}
