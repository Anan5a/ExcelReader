using DataAccess.IRepository;
using Microsoft.Data.SqlClient;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataAccess
{
    public class GroupMembershipRepository : Repository<GroupMembershipModel>, IGroupMembershipRepository
    {
        private readonly string tableName = "group_memberships";

        public GroupMembershipRepository(IMyDbConnection myDbConnection) : base(myDbConnection) { }

        public new long Add(GroupMembershipModel membership)
        {
            long newId = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();

                    string query = $@"
                    INSERT INTO [{tableName}] (
                        [group_id], 
                        [user_id], 
                        [created_at], 
                        [deleted_at]
                    )
                    VALUES (
                        @GroupId, 
                        @UserId, 
                        @CreatedAt, 
                        @DeletedAt
                    );
                    SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GroupId", membership.GroupId);
                        command.Parameters.AddWithValue("@UserId", membership.UserId);
                        command.Parameters.AddWithValue("@CreatedAt", membership.CreatedAt);
                        command.Parameters.AddWithValue("@DeletedAt", membership.DeletedAt ?? (object)DBNull.Value);

                        newId = Convert.ToInt64(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return newId;
        }

        public new GroupMembershipModel? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false)
        {
            GroupMembershipModel? membership = null;

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
                            whereClause.Append($"[{pair.Key}] IS NULL");
                        }
                        else
                        {
                            whereClause.Append($"[{pair.Key}] = @{pair.Key}");
                            parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                        }
                    }

                    string query = $@"
                    SELECT 
                        [group_membership_id], 
                        [group_id], 
                        [user_id], 
                        [created_at], 
                        [deleted_at]
                    FROM [{tableName}]";

                    if (whereClause.Length > 0)
                        query += $" WHERE {whereClause}";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                membership = new GroupMembershipModel
                                {
                                    GroupMembershipId = Convert.ToInt64(reader["group_membership_id"]),
                                    GroupId = Convert.ToInt64(reader["group_id"]),
                                    UserId = Convert.ToInt64(reader["user_id"]),
                                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                    DeletedAt = reader["deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["deleted_at"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return membership;
        }
        public long Count(Dictionary<string, dynamic>? condition = null)
        {
            return Count(tableName, condition);
        }

        public new IEnumerable<GroupMembershipModel> GetAll(Dictionary<string, dynamic>? condition = null,
             bool resolveRelation = false,
             bool lastOnly = false, int n = 10,
             bool whereConditionUseOR = false
        )
        {
            List<GroupMembershipModel> memberships = new List<GroupMembershipModel>();

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
                                whereClause.Append(whereConditionUseOR ? " OR " : " AND ");


                            if (pair.Value == null)
                            {
                                whereClause.Append($"[{pair.Key}] IS NULL");
                            }
                            else
                            {
                                whereClause.Append($"[{pair.Key}] = @{pair.Key}");
                                parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                            }
                        }
                    }

                    string query = $@"
                    SELECT 
                        [group_membership_id], 
                        [group_id], 
                        [user_id], 
                        [created_at], 
                        [deleted_at]
                    FROM [{tableName}]";

                    if (whereClause.Length > 0)
                        query += $" WHERE {whereClause}";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                memberships.Add(new GroupMembershipModel
                                {
                                    GroupMembershipId = Convert.ToInt64(reader["group_membership_id"]),
                                    GroupId = Convert.ToInt64(reader["group_id"]),
                                    UserId = Convert.ToInt64(reader["user_id"]),
                                    CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                    DeletedAt = reader["deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["deleted_at"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return memberships;
        }

        public new bool Update(GroupMembershipModel membership)
        {
            bool isUpdated = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();

                    string query = $@"
                    UPDATE [{tableName}]
                    SET 
                        [group_id] = @GroupId,
                        [user_id] = @UserId,
                        [deleted_at] = @DeletedAt
                    WHERE [group_membership_id] = @GroupMembershipId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GroupId", membership.GroupId);
                        command.Parameters.AddWithValue("@UserId", membership.UserId);
                        command.Parameters.AddWithValue("@DeletedAt", membership.DeletedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@GroupMembershipId", membership.GroupMembershipId);

                        isUpdated = command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return isUpdated;
        }

        public int Remove(int id)
        {
            return base.Remove(tableName, "group_membership_id", id);
        }

        public int RemoveRange(List<int> ids)
        {
            return base.RemoveRange(tableName, "group_membership_id", ids);
        }
    }
}