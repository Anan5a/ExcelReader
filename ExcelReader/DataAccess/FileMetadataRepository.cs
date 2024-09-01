using DataAccess;
using ExcelReader.DataAccess.IRepository;
using ExcelReader.Models;
using ExcelReader.Services;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace ExcelReader.DataAccess
{
    public class FileMetadataRepository : Repository<FileMetadata>, IFileMetadataRepository
    {
        private readonly string tableName = "file_metadata";

        public FileMetadataRepository(IMyDbConnection dbConnection) : base(dbConnection) { }
        public int Remove(int id)
        {
            return base.Remove(tableName, "Id", id);
        }

        public int RemoveRange(List<int> Ids)
        {
            return base.RemoveRange(tableName, "Id", Ids);
        }
        public new ulong Add(FileMetadata fileMetadata)
        {
            ulong newId = 0;

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
                    [created_at], 
                    [updated_at], 
                    [deleted_at]
                )
                VALUES 
                (
                    @FileName,
                    @FileNameSystem,
                    @UserId, 
                    @CreatedAt,
                    @UpdatedAt, 
                    @DeletedAt
                );
            SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", fileMetadata.FileName);
                        command.Parameters.AddWithValue("@FileNameSystem", fileMetadata.FileNameSystem);
                        command.Parameters.AddWithValue("@UserId", fileMetadata.UserId);
                        command.Parameters.AddWithValue("@CreatedAt", fileMetadata.CreatedAt);
                        command.Parameters.AddWithValue("@UpdatedAt", fileMetadata.UpdatedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DeletedAt", fileMetadata.DeletedAt ?? (object)DBNull.Value);

                        newId = Convert.ToUInt64(command.ExecuteScalar());
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

                        whereClause.Append($"f.[{pair.Key}] = @{pair.Key}");
                        parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                    }

                    string query;
                    if (resolveRelation)
                    {
                        query = $@"
                        SELECT f.[id] as Id, 
                               f.[file_name] as FileName,
                               f.[file_name_system] as FileNameSystem,
                               f.[user_id] as UserId, 
                               f.[created_at] as CreatedAt, 
                               f.[updated_at] as UpdatedAt, 
                               f.[deleted_at] as DeletedAt
                        FROM [{tableName}] f
                        INNER JOIN [users] u ON f.[user_id] = u.[id]";
                    }
                    else
                    {
                        query = $@"
                        SELECT f.[id] as Id, 
                               f.[file_name] as FileName,
                               f.[file_name_system] as FileNameSystem,
                               f.[user_id] as UserId, 
                               f.[created_at] as CreatedAt, 
                               f.[updated_at] as UpdatedAt, 
                               f.[deleted_at] as DeletedAt
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
                                    Id = Convert.ToInt64(reader["Id"]),
                                    FileName = Convert.ToString(reader["FileName"]),
                                    FileNameSystem = Convert.ToString(reader["FileNameSystem"]),
                                    UserId = Convert.ToInt64(reader["UserId"]),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UpdatedAt"]),
                                    DeletedAt = reader["DeletedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedAt"])
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

        public new IEnumerable<FileMetadata> GetAll(Dictionary<string, dynamic>? condition = null, bool resolveRelation = false)
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

                            whereClause.Append($"f.[{pair.Key}] = @{pair.Key}");
                            parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                        }
                    }

                    string query;
                    if (resolveRelation)
                    {

                        query = $@"
            SELECT f.[id] as Id, 
                   f.[file_name] as FileName,
                   f.[file_name_system] as FileNameSystem,
                   f.[user_id] as UserId, 
                   f.[created_at] as CreatedAt, 
                   f.[updated_at] as UpdatedAt, 
                   f.[deleted_at] as DeletedAt
            FROM [{tableName}] f
            INNER JOIN [users] u ON f.[user_id] = u.[id]";
                    }
                    else
                    {
                        query = $@"
            SELECT f.[id] as Id, 
                   f.[file_name] as FileName,
                   f.[file_name_system] as FileNameSystem,
                   f.[user_id] as UserId, 
                   f.[created_at] as CreatedAt, 
                   f.[updated_at] as UpdatedAt, 
                   f.[deleted_at] as DeletedAt
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
                                    Id = Convert.ToInt64(row["Id"]),
                                    FileName = Convert.ToString(row["FileName"]),
                                    FileNameSystem = Convert.ToString(row["FileNameSystem"]),
                                    UserId = Convert.ToInt64(row["UserId"]),
                                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                                    UpdatedAt = row["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["UpdatedAt"]),
                                    DeletedAt = row["DeletedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["DeletedAt"])
                                };

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
            WHERE [id] = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", fileMetadata.FileName ?? (object)DBNull.Value);
                        //command.Parameters.AddWithValue("@FileNameSystem", fileMetadata.FileNameSystem);
                        command.Parameters.AddWithValue("@UserId", fileMetadata.UserId);
                        //command.Parameters.AddWithValue("@CreatedAt", fileMetadata.CreatedAt);
                        command.Parameters.AddWithValue("@UpdatedAt", fileMetadata.UpdatedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DeletedAt", fileMetadata.DeletedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Id", fileMetadata.Id);

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

        public ulong Count(Dictionary<string, dynamic>? condition = null)
        {
            return base.Count(tableName, condition);
        }
    }


}
