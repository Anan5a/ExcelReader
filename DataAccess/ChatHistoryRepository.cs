using DataAccess.IRepository;
using Microsoft.Data.SqlClient;
using Models;
using Services;
using System.Text;

namespace DataAccess
{
    public class ChatHistoryRepository : Repository<ChatHistory>, IChatHistoryRepository
    {
        private readonly string tableName = "chat_history";

        public ChatHistoryRepository(IMyDbConnection dbConnection) : base(dbConnection)
        {
        }

        public new long Add(ChatHistory chatHistory)
        {
            try
            {
                using (var connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();
                    var query = @$"
                        INSERT INTO {tableName} (sender_id, receiver_id, content, created_at)
                        VALUES (@AgentId, @CustomerId, @Content, @CreatedAt);
                        SELECT SCOPE_IDENTITY();";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AgentId", chatHistory.SenderId);
                        command.Parameters.AddWithValue("@CustomerId", chatHistory.ReceiverId);
                        command.Parameters.AddWithValue("@Content", chatHistory.Content ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedAt", chatHistory.CreatedAt);

                        return Convert.ToInt64(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
                return 0;
            }
        }

        public new IEnumerable<ChatHistory> GetAll(Dictionary<string, dynamic>? condition = null, bool resolveRelation = false, bool lastOnly = true, int n = 10, bool whereConditionUseOR = false)
        {
            var chatHistories = new List<ChatHistory>();
            try
            {
                using (var connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();
                    var queryBuilder = new StringBuilder($"SELECT TOP ({n}) * FROM {tableName}");
                    var parameters = new List<SqlParameter>();

                    if (condition != null && condition.Count > 0)
                    {
                        queryBuilder.Append(" WHERE ");
                        foreach (var kvp in condition)
                        {
                            if (parameters.Count > 0) queryBuilder.Append(whereConditionUseOR ? " OR " : " AND ");
                            queryBuilder.Append($"[{kvp.Key}] = @{kvp.Key}");
                            parameters.Add(new SqlParameter($"@{kvp.Key}", kvp.Value));
                        }
                    }
                    if (lastOnly)
                    {
                        queryBuilder.Append(" ORDER BY [created_at] DESC");
                    }


                    using (var command = new SqlCommand(queryBuilder.ToString(), connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                chatHistories.Add(new ChatHistory
                                {
                                    ChatHistoryId = reader.GetInt64(reader.GetOrdinal("chat_history_id")),
                                    SenderId = reader.GetInt64(reader.GetOrdinal("sender_id")),
                                    ReceiverId = reader.GetInt64(reader.GetOrdinal("receiver_id")),
                                    Content = reader["content"] as string,
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
            }

            return chatHistories;
        }

        public new ChatHistory? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false)
        {
            try
            {
                using (var connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();
                    var queryBuilder = new StringBuilder($"SELECT * FROM {tableName} WHERE ");
                    var parameters = new List<SqlParameter>();

                    foreach (var kvp in condition)
                    {
                        if (parameters.Count > 0) queryBuilder.Append(" AND ");
                        queryBuilder.Append($"[{kvp.Key}] = @{kvp.Key}");
                        parameters.Add(new SqlParameter($"@{kvp.Key}", kvp.Value));
                    }

                    using (var command = new SqlCommand(queryBuilder.ToString(), connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new ChatHistory
                                {
                                    ChatHistoryId = reader.GetInt64(reader.GetOrdinal("chat_history_id")),
                                    SenderId = reader.GetInt64(reader.GetOrdinal("sender_id")),
                                    ReceiverId = reader.GetInt64(reader.GetOrdinal("receiver_id")),
                                    Content = reader["content"] as string,
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
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

            return null;
        }

        public new ChatHistory? Update(ChatHistory chatHistory)
        {
            throw new NotSupportedException("Cannot update chat. Operation not allowed");
            //try
            //{
            //    using (var connection = new SqlConnection(dbConnection.ConnectionString))
            //    {
            //        connection.Open();
            //        var query = $@"
            //            UPDATE {tableName}
            //            SET sender_id = @AgentId,
            //                receiver_id = @CustomerId,
            //                content = @Content,
            //                created_at = @CreatedAt
            //            WHERE chat_history_id = @ChatHistoryId";

            //        using (var command = new SqlCommand(query, connection))
            //        {
            //            command.Parameters.AddWithValue("@ChatHistoryId", chatHistory.ChatHistoryId);
            //            command.Parameters.AddWithValue("@AgentId", chatHistory.AgentId);
            //            command.Parameters.AddWithValue("@CustomerId", chatHistory.CustomerId);
            //            command.Parameters.AddWithValue("@Content", chatHistory.Content ?? (object)DBNull.Value);
            //            command.Parameters.AddWithValue("@CreatedAt", chatHistory.CreatedAt);

            //            var affectedRows = command.ExecuteNonQuery();
            //            return affectedRows > 0 ? chatHistory : null;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ErrorConsole.Log(ex.Message);
            //    return null;
            //}
        }
        public int Remove(int Id)
        {
            return base.Remove(tableName, "chat_history_id", Id);
        }
        public int RemoveRange(List<int> Ids)
        {
            return base.RemoveRange(tableName, "chat_history_id", Ids);
        }

        public long Count(Dictionary<string, dynamic>? condition = null)
        {
            return base.Count(tableName, condition);
        }
    }
}