using DataAccess.IRepository;
using ExcelReader.Models;
using ExcelReader.Services;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace DataAccess
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly string tableName = "users";

        public UserRepository(IMyDbConnection dbConnection) : base(dbConnection)
        {
        }

        public int Remove(int Id)
        {
            return base.Remove(tableName, "Id", Id);
        }
        public int RemoveRange(List<int> Ids)
        {
            return base.RemoveRange(tableName, "Id", Ids);
        }
        public new ulong Add(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();
                    string sql = $@"INSERT INTO [{tableName}] (
                                                [name], 
                                                [email], 
                                                [password], 
                                                [created_at], 
                                                [updated_at], 
                                                [deleted_at], 
                                                [role_id], 
                                                [password_reset_id], 
                                                [verified_at], 
                                                [status]
                                            ) 
                                            VALUES (
                                                @Name, 
                                                @Email, 
                                                @Password, 
                                                @CreatedAt, 
                                                @UpdatedAt, 
                                                @DeletedAt, 
                                                @RoleId, 
                                                @PasswordResetId, 
                                                @VerifiedAt, 
                                                @Status
                                            ); 
                                            SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {

                        command.Parameters.AddWithValue("@Name", user.Name ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Password", user.Password ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                        command.Parameters.AddWithValue("@UpdatedAt", user.UpdatedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DeletedAt", user.DeletedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@RoleId", user.RoleId);
                        command.Parameters.AddWithValue("@PasswordResetId", user.PasswordResetId ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@VerifiedAt", user.VerifiedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Status", user.Status ?? (object)DBNull.Value);

                        var insertedUserId = Convert.ToUInt64(command.ExecuteScalar());
                        return insertedUserId;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.console(ex.Message);
                return 0;
            }
        }
        public new IEnumerable<User> GetAll(Dictionary<string, dynamic>? condition = null, bool resolveRelation = false)
        {
            List<User> users = new List<User>();
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

                        whereClause.Append($"u.[{pair.Key}] = @{pair.Key}");
                        parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                    }
                }

                // Constructing the query
                string query;
                if (resolveRelation)
                {

                    query = $@"
                SELECT u.[id] as Id, 
                       u.[name] as Name, 
                       u.[email] as Email, 
                       u.[password] as Password, 
                       u.[created_at] as CreatedAt, 
                       u.[updated_at] as UpdatedAt, 
                       u.[deleted_at] as DeletedAt, 
                       u.[role_id] as RoleId, 
                       u.[password_reset_id] as PasswordResetId, 
                       u.[verified_at] as VerifiedAt, 
                       u.[status] as Status,
                       r.[role_name] as RoleName
                FROM [{tableName}] u
                INNER JOIN [roles] r ON u.[role_id] = r.[id]";
                }
                else
                {
                    query = $@"
                SELECT u.[id] as Id, 
                       u.[name] as Name, 
                       u.[email] as Email, 
                       u.[password] as Password, 
                       u.[created_at] as CreatedAt, 
                       u.[updated_at] as UpdatedAt, 
                       u.[deleted_at] as DeletedAt, 
                       u.[role_id] as RoleId, 
                       u.[password_reset_id] as PasswordResetId, 
                       u.[verified_at] as VerifiedAt, 
                       u.[status] as Status,
                FROM [{tableName}] u";
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
                            User user = new User
                            {
                                Id = Convert.ToInt64(row["Id"]),
                                Name = Convert.ToString(row["Name"]),
                                Email = Convert.ToString(row["Email"]),
                                Password = Convert.ToString(row["Password"]),
                                CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                                UpdatedAt = row["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["UpdatedAt"]),
                                DeletedAt = row["DeletedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DeletedAt"]),
                                RoleId = Convert.ToInt64(row["RoleId"]),
                                PasswordResetId = row["PasswordResetId"] == DBNull.Value ? null : Convert.ToString(row["PasswordResetId"]),
                                VerifiedAt = row["VerifiedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["VerifiedAt"]),
                                Status = Convert.ToString(row["Status"]),
                            };

                            if (resolveRelation)
                            {
                                user.Role = new Role
                                {
                                    Id = user.RoleId,
                                    RoleName = Convert.ToString(row["RoleName"]),
                                };
                            }

                            users.Add(user);
                        }
                    }
                }
            }
            return users;
        }

        public new User? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false)
        {
            User? user = null;
            using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
            {
                connection.Open();
                StringBuilder whereClause = new StringBuilder();
                List<SqlParameter> parameters = new List<SqlParameter>();
                foreach (var pair in condition)
                {
                    if (whereClause.Length > 0)
                        whereClause.Append(" AND ");

                    whereClause.Append($"u.[{pair.Key}] = @{pair.Key}");
                    parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                }

                string query;
                if (resolveRelation)
                {
                    query = $@"
                SELECT u.[id] as Id, 
                       u.[name] as Name, 
                       u.[email] as Email, 
                       u.[password] as Password, 
                       u.[created_at] as CreatedAt, 
                       u.[updated_at] as UpdatedAt, 
                       u.[deleted_at] as DeletedAt, 
                       u.[role_id] as RoleId, 
                       u.[password_reset_id] as PasswordResetId, 
                       u.[verified_at] as VerifiedAt, 
                       u.[status] as Status,
                       r.[role_name] as RoleName
                FROM [{{tableName}}] u
                INNER JOIN [roles] r ON u.[role_id] = r.[id]
                WHERE " + whereClause;
                }
                else
                {
                    query = $@"
                SELECT u.[id] as Id, 
                       u.[name] as Name, 
                       u.[email] as Email, 
                       u.[password] as Password, 
                       u.[created_at] as CreatedAt, 
                       u.[updated_at] as UpdatedAt, 
                       u.[deleted_at] as DeletedAt, 
                       u.[role_id] as RoleId, 
                       u.[password_reset_id] as PasswordResetId, 
                       u.[verified_at] as VerifiedAt, 
                       u.[status] as Status,
                FROM [{tableName}] u
                WHERE " + whereClause;
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = Convert.ToInt64(reader["Id"]),
                                Name = Convert.ToString(reader["Name"]),
                                Email = Convert.ToString(reader["Email"]),
                                Password = Convert.ToString(reader["Password"]),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UpdatedAt"]),
                                DeletedAt = reader["DeletedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedAt"]),
                                RoleId = Convert.ToInt64(reader["RoleId"]),
                                PasswordResetId = reader["PasswordResetId"] == DBNull.Value ? null : Convert.ToString(reader["PasswordResetId"]),
                                VerifiedAt = reader["VerifiedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["VerifiedAt"]),
                                Status = Convert.ToString(reader["Status"]),
                            };

                            if (resolveRelation)
                            {
                                user.Role = new Role
                                {
                                    Id = user.RoleId,
                                    RoleName = Convert.ToString(reader["RoleName"]),
                                };
                            }
                        }
                    }
                }
            }
            return user;
        }


        public new User? Update(User existingUser)
        {
            using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
            {
                connection.Open();
                string query = $@"
                    UPDATE [{tableName}]
                    SET [name] = @Name, 
                        [email] = @Email, 
                        [password] = @Password, 
                        [updated_at] = @UpdatedAt, 
                        [deleted_at] = @DeletedAt,
                        [role_id] = @RoleId, 
                        [password_reset_id] = @PasswordResetId, 
                        [verified_at] = @VerifiedAt, 
                        [status] = @Status
                    WHERE [id] = @Id";
                 
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", existingUser.Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", existingUser.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Password", existingUser.Password ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedAt", existingUser.UpdatedAt ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DeletedAt", existingUser.DeletedAt ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@RoleId", existingUser.RoleId);
                    command.Parameters.AddWithValue("@PasswordResetId", existingUser.PasswordResetId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@VerifiedAt", existingUser.VerifiedAt ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Status", existingUser.Status ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Id", existingUser.Id);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return null;
                    }
                }
            }

            return existingUser;
        }

    }
}
