using IRepository;
using Microsoft.Data.SqlClient;
using Models;
using Services;
using System.Data;
using System.Text;

namespace DataAccess
{
    public class FileMetadataRepository : Repository<FileMetadata>, IFileMetadataRepository
    {
        private readonly string tableName = "file_metadata";

        public FileMetadataRepository(IMyDbConnection dbConnection) : base(dbConnection) { }
        public int Remove(int id)
        {
            return Remove(tableName, "file_metadata_id", id);
        }

        public int RemoveRange(List<int> Ids)
        {
            return RemoveRange(tableName, "file_metadata_id", Ids);
        }

        public new long Add(FileMetadata fileMetadata)
        {
            long newId = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();

                    string query = $@"
            INSERT INTO [{tableName}] (
                    [file_name], 
                    [file_name_system],
                    [user_id], 
                    [filesize_bytes],
                    [created_at], 
                    [updated_at], 
                    [deleted_at]
                )
                VALUES 
                (
                    @file_name,
                    @file_name_system,
                    @user_id, 
                    @filesize_bytes,
                    @created_at,
                    @updated_at, 
                    @deleted_at
                );
            SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@file_name", fileMetadata.FileName);
                        command.Parameters.AddWithValue("@file_name_system", fileMetadata.FileNameSystem);
                        command.Parameters.AddWithValue("@user_id", fileMetadata.UserId);
                        command.Parameters.AddWithValue("@filesize_bytes", fileMetadata.FilesizeBytes);
                        command.Parameters.AddWithValue("@created_at", fileMetadata.CreatedAt);
                        command.Parameters.AddWithValue("@updated_at", fileMetadata.UpdatedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@deleted_at", fileMetadata.DeletedAt ?? (object)DBNull.Value);

                        newId = Convert.ToInt64(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
            }

            return newId;
        }

        public new FileMetadata? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false)
        {
            FileMetadata? fileMetadata = null;

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
                            whereClause.Append($"f.[{pair.Key}] IS NULL");

                        }
                        else
                        {
                            whereClause.Append($"f.[{pair.Key}] = @{pair.Key}");

                            parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                        }
                    }

                    string query;
                    if (resolveRelation)
                    {
                        query = $@"
                        SELECT f.[file_metadata_id], 
                               f.[file_name],
                               f.[file_name_system],
                               f.[user_id], 
                               f.[filesize_bytes],
                               f.[created_at], 
                               f.[updated_at], 
                               f.[deleted_at]
                        FROM [{tableName}] f
                        INNER JOIN [users] u ON f.[user_id] = u.[user_id]";
                    }
                    else
                    {
                        query = $@"
                        SELECT f.[file_metadata_id] , 
                               f.[file_name] ,
                               f.[file_name_system],
                               f.[user_id] , 
                               f.[filesize_bytes],
                               f.[created_at] , 
                               f.[updated_at] , 
                               f.[deleted_at] 
                        FROM [{tableName}] f";
                    }

                    if (whereClause.Length > 0)
                    {
                        query += $" WHERE {whereClause}";
                    }

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                fileMetadata = new FileMetadata
                                {
                                    FileMetadataId = Convert.ToInt64(reader["file_metadata_id"]),
                                    FileName = Convert.ToString(reader["file_name"]),
                                    FileNameSystem = Convert.ToString(reader["file_name_system"]),
                                    UserId = Convert.ToInt64(reader["user_id"]),
                                    FilesizeBytes = Convert.ToInt64(reader["filesize_bytes"]),
                                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                    UpdatedAt = reader["updated_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["updated_at"]),
                                    DeletedAt = reader["deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["deleted_at"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
            }

            return fileMetadata;
        }

        public new IEnumerable<FileMetadata> GetAll(Dictionary<string, dynamic>? condition = null, bool resolveRelation = false, bool lastOnly = true, int n = 10, bool whereConditionUseOR = false)
        {
            List<FileMetadata> fileMetadatas = new List<FileMetadata>();

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
                                whereClause.Append($"f.[{pair.Key}] IS NULL");
                            }
                            else
                            {
                                whereClause.Append($"f.[{pair.Key}] = @{pair.Key}");
                                parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                            }
                        }
                    }

                    string query;
                    if (resolveRelation)
                    {

                        query = $@"
            SELECT f.[file_metadata_id], 
                   f.[file_name],
                   f.[file_name_system] ,
                   f.[user_id] , 
                   f.[filesize_bytes],
                   f.[created_at] , 
                   f.[updated_at] , 
                   f.[deleted_at], 
                   u.[name] as user_name
            FROM [{tableName}] f
            INNER JOIN [users] u ON f.[user_id] = u.[user_id]";
                    }
                    else
                    {
                        query = $@"
            SELECT f.[file_metadata_id] , 
                   f.[file_name] ,
                   f.[file_name_system] ,
                   f.[user_id] , 
                   f.[filesize_bytes],
                   f.[created_at] , 
                   f.[updated_at] , 
                   f.[deleted_at] 
            FROM [{tableName}] f";

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
                                FileMetadata fileMetadata = new FileMetadata
                                {
                                    FileMetadataId = Convert.ToInt64(row["file_metadata_id"]),
                                    FileName = Convert.ToString(row["file_name"]),
                                    FileNameSystem = Convert.ToString(row["file_name_system"]),
                                    UserId = Convert.ToInt64(row["user_id"]),
                                    FilesizeBytes = Convert.ToInt64(row["filesize_bytes"]),
                                    CreatedAt = Convert.ToDateTime(row["created_at"]),
                                    UpdatedAt = row["updated_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["updated_at"]),
                                    DeletedAt = row["deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["deleted_at"])
                                };


                                if (resolveRelation)
                                {
                                    User user = new User
                                    {
                                        UserId = Convert.ToInt64(row["user_id"]),
                                        Name = Convert.ToString(row["user_name"]),
                                    };
                                    fileMetadata.User = user;
                                }

                                fileMetadatas.Add(fileMetadata);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
            }
            return fileMetadatas;
        }

        public new FileMetadata? Update(FileMetadata fileMetadata)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();
                    string query = $@"
            UPDATE [{tableName}]
            SET [file_name] = @FileName, 
                [user_id] = @UserId, 
                [updated_at] = @UpdatedAt, 
                [deleted_at] = @DeletedAt
            WHERE [file_metadata_id] = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", fileMetadata.FileName ?? (object)DBNull.Value);
                        //command.Parameters.AddWithValue("@FileNameSystem", fileMetadata.FileNameSystem);
                        command.Parameters.AddWithValue("@UserId", fileMetadata.UserId);
                        //command.Parameters.AddWithValue("@CreatedAt", fileMetadata.CreatedAt);
                        command.Parameters.AddWithValue("@UpdatedAt", fileMetadata.UpdatedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DeletedAt", fileMetadata.DeletedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Id", fileMetadata.FileMetadataId);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return null;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
            }


            return fileMetadata;
        }

        public long Count(Dictionary<string, dynamic>? condition = null)
        {
            return Count(tableName, condition);
        }
    }


}