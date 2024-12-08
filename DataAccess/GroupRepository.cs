using DataAccess.IRepository;
using Microsoft.Data.SqlClient;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class GroupRepository : Repository<GroupModel>, IGroupRepository
    {
        private readonly string tableName = "groups";

        public GroupRepository(IMyDbConnection myDbConnection) : base(myDbConnection) { }


        public new long Add(GroupModel groupModel)
        {
            long newId = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();

                    string query = @"
                INSERT INTO [groups] (
                    [group_name],
                    [created_at],
                    [deleted_at]
                )
                VALUES 
                (
                    @group_name,
                    @created_at,
                    @deleted_at
                );
                SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@group_name", groupModel.GroupName);
                        command.Parameters.AddWithValue("@created_at", groupModel.CreatedAt);
                        command.Parameters.AddWithValue("@deleted_at", groupModel.DeletedAt ?? (object)DBNull.Value);

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

        public new GroupModel? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false)
        {
            GroupModel? groupModel = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();

                    // Build WHERE clause
                    StringBuilder whereClause = new StringBuilder();
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    foreach (var pair in condition)
                    {
                        if (whereClause.Length > 0)
                            whereClause.Append(" AND ");

                        if (pair.Value == null)
                        {
                            whereClause.Append($"g.[{pair.Key}] IS NULL");
                        }
                        else
                        {
                            whereClause.Append($"g.[{pair.Key}] = @{pair.Key}");
                            parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                        }
                    }

                    // Construct base query
                    string query = @"
                SELECT 
                    g.[group_id], 
                    g.[group_name], 
                    g.[created_at], 
                    g.[deleted_at]
                FROM [groups] g";

                    // Add WHERE clause if conditions exist
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
                                groupModel = new GroupModel
                                {
                                    GroupId = Convert.ToInt64(reader["group_id"]),
                                    GroupName = Convert.ToString(reader["group_name"]),
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
                ErrorConsole.Log(ex.Message); // Consider logging full exception details.
            }

            return groupModel;
        }

        public new IEnumerable<GroupModel> GetAll(
             Dictionary<string, dynamic>? condition = null,
             bool lastOnly=false,
             bool resolveRelation = false,
             int n = 10,
             bool whereConditionUseOR = false
        )
        {
            List<GroupModel> groups = new List<GroupModel>();

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
                                whereClause.Append($"g.[{pair.Key}] IS NULL");
                            }
                            else
                            {
                                whereClause.Append($"g.[{pair.Key}] = @{pair.Key}");
                                parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                            }
                        }
                    }

                    // Base query for groups (no join if resolveRelation is false)
                    string query;
                    if (resolveRelation)
                    {

                        query = @"
                SELECT 
                    g.[group_id], 
                    g.[group_name], 
                    g.[created_at], 
                    g.[deleted_at],
                    m.[group_membership_id],
                    m.[user_id] AS membership_user_id,
                    m.[created_at] AS membership_created_at,
                    m.[deleted_at] AS membership_deleted_at
                FROM [groups] g
                LEFT JOIN [group_memberships] m ON g.[group_id] = m.[group_id]";

                    }
                    else
                    {
                        query = @"
                SELECT 
                    g.[group_id], 
                    g.[group_name], 
                    g.[created_at], 
                    g.[deleted_at],
                FROM [groups] g";
                    }

                    // Add WHERE clause if conditions exist
                    if (whereClause.Length > 0)
                    {
                        query += $" WHERE {whereClause}";
                    }

                    // Add pagination
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            var groupDictionary = new Dictionary<long, GroupModel>();

                            foreach (DataRow row in dataTable.Rows)
                            {
                                long groupId = Convert.ToInt64(row["group_id"]);
                                if (!groupDictionary.ContainsKey(groupId))
                                {
                                    GroupModel group = new GroupModel
                                    {
                                        GroupId = groupId,
                                        GroupName = Convert.ToString(row["group_name"]),
                                        CreatedAt = Convert.ToDateTime(row["created_at"]),
                                        DeletedAt = row["deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["deleted_at"]),
                                        Members = resolveRelation ? new List<GroupMembershipModel>() : null,
                                    };

                                    groupDictionary[groupId] = group;
                                }

                                // Add members if resolveRelation is true
                                if (resolveRelation && row["membership_user_id"] != DBNull.Value)
                                {
                                    GroupMembershipModel member = new GroupMembershipModel
                                    {
                                        GroupMembershipId = Convert.ToInt64(row["group_membership_id"]),
                                        GroupId = groupId,
                                        UserId = Convert.ToInt64(row["membership_user_id"]),
                                        CreatedAt = Convert.ToDateTime(row["membership_created_at"]),
                                        DeletedAt = row["membership_deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["membership_deleted_at"]),
                                    };

                                    groupDictionary[groupId]?.Members?.Add(member);
                                }
                            }

                            groups.AddRange(groupDictionary.Values);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
            }

            return groups;
        }


        public IEnumerable<GroupModel> GetGroupsWithMembersAndDetails(
                Dictionary<string, dynamic>? condition = null,
                bool whereConditionUseOR = false)
        {
            List<GroupModel> groups = new List<GroupModel>();

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
                                whereClause.Append($"g.[{pair.Key}] IS NULL");
                            }
                            else
                            {
                                whereClause.Append($"g.[{pair.Key}] = @{pair.Key}");
                                parameters.Add(new SqlParameter($"@{pair.Key}", pair.Value));
                            }
                        }
                    }

                    // Base query for groups and their memberships with or without users based on resolveRelation
                    string query = @"
                SELECT 
                    g.[group_id], 
                    g.[group_name], 
                    g.[created_at], 
                    g.[deleted_at],
                    m.[group_membership_id],
                    m.[user_id] AS membership_user_id,
                    m.[created_at] AS membership_created_at,
                    m.[deleted_at] AS membership_deleted_at,
                    u.[user_id] AS user_id,
                    u.[name] AS user_name,
                    u.[email] AS user_email,
                    u.[role_id] AS user_role_id,
                    u.[status] AS user_status
                    FROM [groups] g
                    LEFT JOIN [group_memberships] m ON g.[group_id] = m.[group_id]
                    LEFT JOIN [users] u ON m.[user_id] = u.[user_id]";


                    // Add WHERE clause if conditions exist
                    if (whereClause.Length > 0)
                    {
                        query += $" WHERE {whereClause}";
                    }

                    // Add pagination

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            var groupDictionary = new Dictionary<long, GroupModel>();

                            foreach (DataRow row in dataTable.Rows)
                            {
                                long groupId = Convert.ToInt64(row["group_id"]);
                                if (!groupDictionary.ContainsKey(groupId))
                                {
                                    GroupModel group = new GroupModel
                                    {
                                        GroupId = groupId,
                                        GroupName = Convert.ToString(row["group_name"]),
                                        CreatedAt = Convert.ToDateTime(row["created_at"]),
                                        DeletedAt = row["deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["deleted_at"]),
                                        Members = new List<GroupMembershipModel>() // Initialize members list
                                    };

                                    groupDictionary[groupId] = group;
                                }

                                // If members are being resolved, add them to the group
                                if (row["membership_user_id"] != DBNull.Value)
                                {
                                    GroupMembershipModel member = new GroupMembershipModel
                                    {
                                        GroupMembershipId = Convert.ToInt64(row["group_membership_id"]),
                                        GroupId = groupId,
                                        UserId = Convert.ToInt64(row["membership_user_id"]),
                                        CreatedAt = Convert.ToDateTime(row["membership_created_at"]),
                                        DeletedAt = row["membership_deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["membership_deleted_at"]),
                                    };

                                    member.UserDetails = new User
                                    {
                                        UserId = Convert.ToInt64(row["user_id"]),
                                        Name = Convert.ToString(row["user_name"]),
                                        Email = Convert.ToString(row["user_email"]),
                                        RoleId = Convert.ToInt64(row["user_role_id"]),
                                        Status = Convert.ToString(row["user_status"])
                                    };


                                    groupDictionary[groupId]?.Members?.Add(member);
                                }
                            }

                            groups.AddRange(groupDictionary.Values);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
            }

            return groups;
        }


        public new GroupModel? Update(GroupModel groupModel)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbConnection.ConnectionString))
                {
                    connection.Open();

                    string query = $@"
                UPDATE [{tableName}]
                SET [group_name] = @GroupName, 
                    [user_id] = @UserId, 
                    [updated_at] = @UpdatedAt, 
                    [deleted_at] = @DeletedAt
                WHERE [group_id] = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GroupName", groupModel.GroupName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DeletedAt", groupModel.DeletedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@UpdatedAt", groupModel.UpdatedAt ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Id", groupModel.GroupId);

                        int rowsAffected = command.ExecuteNonQuery();

                        // If no rows were affected, return null
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

            return groupModel;
        }


        public int Remove(int Id)
        {
            return base.Remove(tableName, "group_id", Id);
        }
        public int RemoveRange(List<int> Ids)
        {
            return base.RemoveRange(tableName, "group_id", Ids);
        }
        public long Count(Dictionary<string, dynamic>? condition = null)
        {
            return Count(tableName, condition);
        }
    }
}