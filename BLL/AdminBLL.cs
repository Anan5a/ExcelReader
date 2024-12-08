using DataAccess.IRepository;
using IRepository;
using Models;
using Models.DTOs;
using Utility;

namespace BLL
{
    public class AdminBLL
    {
        public static dynamic NewUserConfig(IUserRepository _userRepository, IRoleRepository _roleRepository, long userId)
        {
            IEnumerable<Role> assignableRoles = GetAssignableRoles(_userRepository, _roleRepository, userId);
            if (assignableRoles.Count() == 0)
            {
                //no role? something is very wrong
                return BLLReturnEnum.ACTION_ERROR;
            }

            return new
            {
                roles = assignableRoles
            };
        }

        public static BLLReturnEnum CreateUser(IUserRepository _userRepository, IRoleRepository _roleRepository, long userId, AdminNewUserDTO adminNewUserDto)
        {
            //verify role validity
            IEnumerable<Role> assignableRoles = GetAssignableRoles(_userRepository, _roleRepository, userId);



            if (assignableRoles.Count() == 0)
            {
                //no role? something is very wrong
                return BLLReturnEnum.Admin_ADMIN_NO_ROLE_LIST;

            }

            if (assignableRoles.FirstOrDefault(role => role.RoleId == adminNewUserDto.RoleId, null) == null)
            {
                //invalid role was provided :/
                return BLLReturnEnum.Admin_ADMIN_CREATE_USER_INVALID_ROLE;

            }

            User user = new User
            {
                Email = adminNewUserDto.Email,
                Name = adminNewUserDto.Name,
                Password = PasswordManager.HashPassword(adminNewUserDto.Password),
                RoleId = adminNewUserDto.RoleId,
                PasswordResetId = null,
                Status = UserStatus.OK,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                VerifiedAt = null,
            };
            var createStatus = _userRepository.Add(user);

            if (createStatus == 0)
            {
                return BLLReturnEnum.User_USER_NO_USER_CREATED;
            }
            //todo: add email verification process

            return BLLReturnEnum.ACTION_OK;


        }

        private static IEnumerable<Role> GetAssignableRoles(IUserRepository _userRepository, IRoleRepository _roleRepository, long userId)
        {
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["user_id"] = userId;

            var user = _userRepository.Get(condition, resolveRelation: true);

            var availableRoles = _roleRepository.GetAll();

            IEnumerable<Role> assignableRoles = new List<Role>();

            if (user.Role.RoleName.Equals("admin"))
            {
                assignableRoles = availableRoles.Where(role => role.RoleName.Equals("user"));
            }
            else if (user.Role.RoleName.Equals("super_admin"))
            {
                assignableRoles = availableRoles.Where(role => role.RoleName.Equals("user") || role.RoleName.Equals("admin"));

            }
            return assignableRoles;
        }


        /// Groups
        /// 
        public static BLLReturnEnum CreateGroup(IGroupRepository _groupRepository, CreateGroupDTO createGroupDTO)
        {
            GroupModel groupModel = new GroupModel
            {
                GroupName = createGroupDTO.GroupName,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
            };
            var createStatus = _groupRepository.Add(groupModel);

            if (createStatus == 0)
            {
                return BLLReturnEnum.Group_GROUP_NO_GROUP_CREATED;
            }

            return BLLReturnEnum.ACTION_OK;

        }
        public static BLLReturnEnum CreateGroupMembership(IGroupMembershipRepository _groupMembershipRepository, CreateGroupMembershipDTO createGroupMembershipDTO)
        {
            GroupMembershipModel groupMembershipModel = new()
            {
                GroupId = createGroupMembershipDTO.GroupId,
                UserId = createGroupMembershipDTO.UserId,
                CreatedAt = DateTime.Now,
            };
            var createStatus = _groupMembershipRepository.Add(groupMembershipModel);

            if (createStatus == 0)
            {
                return BLLReturnEnum.Group_GROUP_NO_MEMBERSHIP_CREATED;
            }

            return BLLReturnEnum.ACTION_OK;

        }

        public static IEnumerable<GroupModel> ListGroups(IGroupRepository _groupRepository, string? page)
        {
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            var list = _groupRepository.GetAll(condition, resolveRelation: false);
            return list;
        }

    }
}