using DataAccess.IRepository;
using Google.Apis.Auth;
using IRepository;
using Microsoft.Extensions.Configuration;
using Models;
using Models.DTOs;
using Services;
using Utility;

namespace BLL
{
    public class UserBLL
    {
        public static long Dashboard(IFileMetadataRepository _fileMetadataRepository, long userId)
        {

            //find user and user files
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["user_id"] = userId;
            condition["deleted_at"] = null;

            var userFileCount = _fileMetadataRepository.Count(condition);

            return userFileCount;
        }

        public static BLLReturnEnum ChangePassword(IUserRepository _userRepository, long userId, ChangePasswordDTO changePasswordDTO)
        {

            //find user and user files
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["id"] = userId;

            var user = _userRepository.Get(condition);

            if (user == null)
            {
                return BLLReturnEnum.User_USER_NOT_FOUND;
            }


            //verify old password

            if (!PasswordManager.VerifyPassword(changePasswordDTO.oldPassword, user.Password))
            {
                return BLLReturnEnum.User_USER_AUTH_FAILED;
            }

            user.Password = PasswordManager.HashPassword(changePasswordDTO.newPassword);
            user.UpdatedAt = DateTime.Now;

            var updateStatus = _userRepository.Update(user);
            if (updateStatus == null)
            {
                return BLLReturnEnum.ACTION_ERROR;
            }

            return BLLReturnEnum.ACTION_OK;
        }

        // returns AuthResponse or BLLReturnEnum
        public static dynamic Login(IConfiguration _configuration, IUserRepository _userRepository, UserLoginDTO loginDto)
        {
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["email"] = loginDto.Email;

            User? existingUser = _userRepository.Get(condition, resolveRelation: true);

            if (existingUser == null)
            {
                return BLLReturnEnum.User_USER_NOT_FOUND;
            }
            if (!PasswordManager.VerifyPassword(loginDto.Password, existingUser.Password))
            {
                return BLLReturnEnum.User_USER_AUTH_FAILED;
            }

            if (existingUser.Status != UserStatus.OK)
            {
                return BLLReturnEnum.User_USER_ACCOUNT_ERROR;
            }
            if (existingUser.DeletedAt != null)
            {
                return BLLReturnEnum.User_USER_ACCOUNT_DELETED;
            }

            var token = JwtAuthService.GenerateJwtToken(existingUser, _configuration);

            return new AuthResponse { Token = token, user = existingUser };

        }

        public static BLLReturnEnum Signup(IUserRepository _userRepository, UserSignUpDTO signupDto)
        {
            User user = new User
            {
                Email = signupDto.Email,
                Name = signupDto.Name,
                Password = PasswordManager.HashPassword(signupDto.Password),
                RoleId = 1, //todo: load dynamically
                PasswordResetId = null,
                Status = UserStatus.OK,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                VerifiedAt = DateTime.Now,
            };
            var createStatus = _userRepository.Add(user);

            if (createStatus == 0)
            {
                return BLLReturnEnum.ACTION_ERROR;
            }
            return BLLReturnEnum.ACTION_OK;
        }

        public static dynamic SocialAuth(IConfiguration _configuration, IUserRepository _userRepository, SocialAuthDTO socialAuthDto)
        {
            //verify the id token

            GoogleJsonWebSignature.Payload? payload;
            try
            {
                string idToken = socialAuthDto.IdToken;
                string clientId = _configuration["ExternalAPIs:google:web:client_id"];
                payload = GoogleApiAuth.VerifyIdTokenAsync(idToken, clientId).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                return BLLReturnEnum.ACTION_ERROR;
            }

            //end verify

            //if user exist by email login

            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["email"] = payload.Email;

            User? existingUser = _userRepository.Get(condition, resolveRelation: true);
            if (existingUser != null)
            {
                if (existingUser.SocialLogin == null)
                {
                    //update users social login info
                    _userRepository.Update(existingUser);

                }

                var token = JwtAuthService.GenerateJwtToken(existingUser, _configuration);
                return new AuthResponse { Token = token, user = existingUser };
            }

            //create new user

            User user = new User
            {
                Email = payload.Email,
                Name = payload.Name,
                Password = PasswordManager.HashPassword(payload.JwtId),
                RoleId = 1, //todo: load dynamically
                PasswordResetId = null,
                Status = UserStatus.OK,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                VerifiedAt = DateTime.Now,
                SocialLogin = new { uid = payload.Subject },
            };
            var createStatus = _userRepository.Add(user);

            if (createStatus == 0)
            {
                return BLLReturnEnum.ACTION_ERROR;
            }
            user.Id = Convert.ToInt64(createStatus);
            user.Role = new Role { Id = 1, RoleName = "user" };

            var token2 = JwtAuthService.GenerateJwtToken(user, _configuration);
            return new AuthResponse { Token = token2, user = user };
        }

        public static IEnumerable<User> UsersById(IUserRepository _userRepository, IEnumerable<long> userIds, bool resolveRelation = true)
        {
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["id"] = userIds;

            return _userRepository.GetAll(condition, resolveRelation: resolveRelation);
        }

        public static bool IsUserAdmin(IUserRepository _userRepository, long userId)
        {
            var user = UserBLL.UsersById(_userRepository, new List<long> { userId }).FirstOrDefault();
            return user?.Role?.RoleName != UserRoles.Admin || user?.Role?.RoleName != UserRoles.SuperAdmin;
        }
        public static bool IsUserSuperAdmin(IUserRepository _userRepository, long userId)
        {
            var user = UserBLL.UsersById(_userRepository, new List<long> { userId }).FirstOrDefault();
            return user?.Role?.RoleName != UserRoles.SuperAdmin;
        }


        public static void GetChatHistoryByUserIdAndAgentId(IChatHistoryRepository _chatHistoryRepository, long receiverId, out IEnumerable<ChatHistory> messages, long? senderId = null)
        {
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["receiver_id"] = receiverId;
            if (senderId != null)
            {
                condition["sender_id"] = senderId;
            }
            messages = _chatHistoryRepository.GetAll(condition, lastOnly: true, n: 10, whereConditionUseOR: true);
        }

    }
}
