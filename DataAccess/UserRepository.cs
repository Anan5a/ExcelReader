using IRepository;
using Microsoft.Data.SqlClient;
using Models;
using Services;
using System.Data;
using System.Text;
using System.Text.Json;

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
            return base.Remove(tableName, "id", Id);
        }
        public int RemoveRange(List<int> Ids)
        {
            return base.RemoveRange(tableName, "id", Ids);
        }
        public new long Add(User user)
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
                                                [status],
                                                [social_login]
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
                                                @Status,
                                                @SocialLogin
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
                        command.Parameters.AddWithValue("@SocialLogin", user.SocialLogin != null ? JsonSerializer.Serialize(user.SocialLogin) : (object)DBNull.Value);

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
        public new IEnumerable<User> GetAll(Dictionary<string, dynamic>? condition = null, bool resolveRelation = false)
        {
            List<User> users = new List<User>();

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
                                whereClause.Append($"u.[{pair.Key}] IS NULL");

                            }
                            else
                            {
                                whereClause.Append($"u.[{pair.Key}] = @{pair.Key}");

                                parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                            }
                        }
                    }

                    // Constructing the query
                    string query;
                    if (resolveRelation)
                    {

                        query = $@"
                SELECT u.[id], 
                       u.[name], 
                       u.[email], 
                       u.[password] , 
                       u.[created_at] , 
                       u.[updated_at] , 
                       u.[deleted_at] , 
                       u.[role_id] , 
                       u.[password_reset_id] , 
                       u.[verified_at] , 
                       u.[status] ,
                       u.[social_login] ,
                       r.[role_name] 
                FROM [{tableName}] u
                INNER JOIN [roles] r ON u.[role_id] = r.[id]";
                    }
                    else
                    {
                        query = $@"
                SELECT u.[id] , 
                       u.[name] , 
                       u.[email] , 
                       u.[password] , 
                       u.[created_at] , 
                       u.[updated_at] , 
                       u.[deleted_at] , 
                       u.[role_id] , 
                       u.[password_reset_id] , 
                       u.[verified_at] , 
                       u.[status] ,
                       u.[social_login] 
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
                                    Id = Convert.ToInt64(row["id"]),
                                    Name = Convert.ToString(row["name"]),
                                    Email = Convert.ToString(row["email"]),
                                    Password = Convert.ToString(row["password"]),
                                    CreatedAt = Convert.ToDateTime(row["created_at"]),
                                    UpdatedAt = row["updated_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["updated_at"]),
                                    DeletedAt = row["deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["deleted_at"]),
                                    RoleId = Convert.ToInt64(row["role_id"]),
                                    PasswordResetId = row["password_reset_id"] == DBNull.Value ? null : Convert.ToString(row["password_reset_id"]),
                                    VerifiedAt = row["verified_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["verified_at"]),
                                    Status = Convert.ToString(row["status"]),
                                    SocialLogin = row["social_login"] == DBNull.Value ? null : JsonSerializer.Deserialize<object>(Convert.ToString(row["social_login"]))
                                };

                                if (resolveRelation)
                                {
                                    user.Role = new Role
                                    {
                                        Id = user.RoleId,
                                        RoleName = Convert.ToString(row["role_name"]),
                                    };
                                }

                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorConsole.Log(ex.Message); }

            return users;
        }

        public new User? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false)
        {
            User? user = null;
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
                            whereClause.Append($"u.[{pair.Key}] IS NULL");

                        }
                        else
                        {
                            whereClause.Append($"u.[{pair.Key}] = @{pair.Key}");

                            parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                        }
                    }

                    string query;
                    if (resolveRelation)
                    {
                        query = $@"
                SELECT u.[id] , 
                        u.[name], 
                       u.[email], 
                       u.[password] , 
                       u.[created_at] , 
                       u.[updated_at] , 
                       u.[deleted_at] , 
                       u.[role_id] , 
                       u.[password_reset_id] , 
                       u.[verified_at] , 
                       u.[status] ,
                       u.[social_login],
                       r.[role_name] 
                FROM [{tableName}] u
                INNER JOIN [roles] r ON u.[role_id] = r.[id]
                WHERE " + whereClause;
                    }
                    else
                    {
                        query = $@"
                SELECT u.[id] , 
                        u.[name], 
                       u.[email], 
                       u.[password] , 
                       u.[created_at] , 
                       u.[updated_at] , 
                       u.[deleted_at] , 
                       u.[role_id] , 
                       u.[password_reset_id] , 
                       u.[verified_at] , 
                       u.[social_login],
                       u.[status] 
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
                                    Id = Convert.ToInt64(reader["id"]),
                                    Name = Convert.ToString(reader["name"]),
                                    Email = Convert.ToString(reader["email"]),
                                    Password = Convert.ToString(reader["password"]),
                                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                    UpdatedAt = reader["updated_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["updated_at"]),
                                    DeletedAt = reader["deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["deleted_at"]),
                                    RoleId = Convert.ToInt64(reader["role_id"]),
                                    PasswordResetId = reader["password_reset_id"] == DBNull.Value ? null : Convert.ToString(reader["password_reset_id"]),
                                    VerifiedAt = reader["verified_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["verified_at"]),
                                    Status = Convert.ToString(reader["status"]),
                                    SocialLogin = reader["social_login"] == DBNull.Value ? null : JsonSerializer.Deserialize<object>(Convert.ToString(reader["social_login"]))

                                };

                                if (resolveRelation)
                                {
                                    user.Role = new Role
                                    {
                                        Id = user.RoleId,
                                        RoleName = Convert.ToString(reader["role_name"]),
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ErrorConsole.Log(ex.Message); }

            return user;
        }


        public new User? Update(User existingUser)
        {
            try
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
                        [status] = @Status,
                        [social_login] = @SocialLogin
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
                        command.Parameters.AddWithValue("@SocialLogin", existingUser.SocialLogin == null ? (object)DBNull.Value : JsonSerializer.Serialize(existingUser.SocialLogin));
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
