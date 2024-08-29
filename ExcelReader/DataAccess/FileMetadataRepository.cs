using DataAccess;
using DocumentFormat.OpenXml.Office2010.Excel;
using ExcelReader.DataAccess.IRepository;
using ExcelReader.Models;
using Microsoft.Data.SqlClient;

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

            using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
            {
                connection.Open();

                string query = $@"
            INSERT INTO [{tableName}] (
                    [file_name], 
                    [file_name_system],
                    [file_owner], 
                    [created_at], 
                    [updated_at], 
                    [deleted_at]
                )
                VALUES 
                (
                    @FileName,
                    @FileNameSystem,
                    @FileOwner, 
                    @CreatedAt,
                    @UpdatedAt, 
                    @DeletedAt
                );
            SELECT SCOPE_IDENTITY();";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FileName", fileMetadata.FileName);
                    command.Parameters.AddWithValue("@FileNameSystem", fileMetadata.FileNameSystem);
                    command.Parameters.AddWithValue("@FileOwner", fileMetadata.FileOwner);
                    command.Parameters.AddWithValue("@CreatedAt", fileMetadata.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", fileMetadata.UpdatedAt ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DeletedAt", fileMetadata.DeletedAt ?? (object)DBNull.Value);

                    newId = Convert.ToUInt64(command.ExecuteScalar());
                }
            }

            return newId;
        }


        public new FileMetadata? Get(Dictionary<string, dynamic> condition, bool resolveRelation)
        {
            throw new NotImplementedException();
        }

        public new IEnumerable<FileMetadata> GetAll(Dictionary<string, dynamic>? condition, bool resolveRelation)
        {
            throw new NotImplementedException();
        }

        public new FileMetadata? Update(FileMetadata fileMetadata)
        {
            throw new NotImplementedException();
        }
    }


}
