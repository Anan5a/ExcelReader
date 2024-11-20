using IRepository;
using Microsoft.Data.SqlClient;
using Models;
using Services;
using System.Data;
using System.Text;

namespace DataAccess
{
    public class RoleRepository : Repository<Role>, IRoleRepository
    {
        private readonly string tableName = "roles";

        public RoleRepository(IMyDbConnection dbConnection) : base(dbConnection)
        {
        }

        public int Remove(int Id)
        {
            return base.Remove(tableName, "id", Id);
        }
        public int RemoveRange(List<int> Ids)
        {
            return base.RemoveRange(tableName, "id", Ids);
        }
        public new long Add(Role role)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();
                    string sql = $@"INSERT INTO [{tableName}] (
                                                [role_name]
                                            ) 
                                            VALUES (
                                                @RoleName
                                            ); 
                                            SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@RoleName", role.RoleName);
                        var insertedUserId = Convert.ToInt64(command.ExecuteScalar());
                        return insertedUserId;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
                return 0;
            }
        }
        public new IEnumerable<Role> GetAll(Dictionary<string, dynamic>? condition = null, bool resolveRelation = false, bool lastOnly = true, int n = 10, bool whereConditionUseOR = false)
        {
            List<Role> roles = new List<Role>();

            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();

                    StringBuilder whereClause = new StringBuilder();
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    if (condition != null)
                    {
                        foreach (var pair in condition)
                        {
                            if (whereClause.Length > 0)
                                whereClause.Append(" AND ");

                            if (pair.Value == null)
                            {
                                whereClause.Append($"r.[{pair.Key}] IS @{pair.Key}");

                            }
                            else
                            {
                                whereClause.Append($"r.[{pair.Key}] = @{pair.Key}");

                            }
                            parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                        }
                    }

                    // Constructing the query
                    string query;
                    if (resolveRelation)
                    {

                        query = $@"
                SELECT r.[id] as Id, 
                       r.[role_name] as RoleName
                FROM [{tableName}] r";
                    }
                    else
                    {
                        query = $@"
                SELECT r.[id] as Id, 
                       r.[role_name] as RoleName
                FROM [{tableName}] r";
                    }

                    if (whereClause.Length > 0)
                    {
                        query += $" WHERE {whereClause}";
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            foreach (DataRow row in dataTable.Rows)
                            {
                                Role role = new Role
                                {
                                    Id = Convert.ToInt64(row["Id"]),
                                    RoleName = Convert.ToString(row["RoleName"]),
                                };

                                if (resolveRelation)
                                {

                                }

                                roles.Add(role);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorConsole.Log(ex.Message); }

            return roles;
        }

        public new Role? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false)
        {
            Role? role = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();
                    StringBuilder whereClause = new StringBuilder();
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    foreach (var pair in condition)
                    {
                        if (whereClause.Length > 0)
                            whereClause.Append(" AND ");

                        if (pair.Value == null)
                        {
                            whereClause.Append($"r.[{pair.Key}] IS @{pair.Key}");

                        }
                        else
                        {
                            whereClause.Append($"r.[{pair.Key}] = @{pair.Key}");

                        }
                        parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                    }

                    string query;
                    if (resolveRelation)
                    {
                        query = $@"
                SELECT r.[id] as Id, 
                       r.[role_name] as RoleName
                FROM [{tableName}] r
                WHERE " + whereClause;
                    }
                    else
                    {
                        query = $@"
                SELECT r.[id] as Id, 
                       r.[role_name] as RoleName
                FROM [{tableName}] r
                WHERE " + whereClause;
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                role = new Role
                                {
                                    Id = Convert.ToInt64(reader["Id"]),
                                    RoleName = Convert.ToString(reader["RoleName"]),
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorConsole.Log(ex.Message); }

            return role;
        }


        public new Role? Update(Role existingRole)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();
                    string query = $@"
                    UPDATE [{tableName}]
                    SET [role_name] = @RoleName 
                    WHERE [id] = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RoleName", existingRole.RoleName);
                        command.Parameters.AddWithValue("@Id", existingRole.Id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return null;
                        }
                    }
                }

                return existingRole;
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
                return null;
            }

        }

        public long Count(Dictionary<string, dynamic>? condition = null)
        {
            return base.Count(tableName, condition);
        }
    }
}
