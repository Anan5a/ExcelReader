using DataAccess.IRepository;
using ExcelReader.DataAccess.IRepository;
using ExcelReader.Models;
using ExcelReader.Models.DTOs;
using ExcelReader.Services;
using ExcelReader.Utility;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExcelReader.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class UserController : Controller
    {
        private IWebHostEnvironment _webHostEnvironment;
        private IConfiguration _configuration;
        private IUserRepository _userRepository;
        private readonly IFileMetadataRepository _fileMetadataRepository;

        public UserController(IWebHostEnvironment webHostEnvironment, IUserRepository userRepository, IConfiguration configuration, IFileMetadataRepository fileMetadataRepository)
        {
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
            _configuration = configuration;
            _fileMetadataRepository = fileMetadataRepository;

        }
        [HttpGet]
        [Route("dashboard")]
        [Authorize(Roles = "user, admin, super_admin")]
        public async Task<ActionResult<ResponseModel<object>>> Dashboard()
        {
            //get authenticate user's Dashboard
            //if (Role.Identity != null && !Role.Identity.IsAuthenticated)
            //{

            //}
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            //find user and user files
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["user_id"] = userId;

            var userFileCount = _fileMetadataRepository.Count(condition);

            return Ok(CustomResponseMessage.OkCustom<object>("Successful query", new { fileCount = userFileCount }));
        }


        [HttpPost]
        [Route("change-password")]
        [Authorize(Roles = "user, admin, super_admin")]
        public async Task<ActionResult<ResponseModel<string?>>> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            //get authenticate user's Dashboard
            //if (Role.Identity != null && !Role.Identity.IsAuthenticated)
            //{

            //}
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            //find user and user files
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["id"] = userId;

            var user = _userRepository.Get(condition);

            if (user == null)
            {
                return NotFound(CustomResponseMessage.ErrorCustom("Not found", "User was not found!"));
            }


            //verify old password

            if (!PasswordManager.VerifyPassword(changePasswordDTO.oldPassword, user.Password))
            {
                return StatusCode(StatusCodes.Status403Forbidden, CustomResponseMessage.ErrorCustom("Forbidden", "Failed to verify old password."));
            }

            user.Password = PasswordManager.HashPassword(changePasswordDTO.newPassword);
            user.UpdatedAt = DateTime.Now;

            var updateStatus = _userRepository.Update(user);
            if (updateStatus == null)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("update error", "Failed to update password. Try again later."));
            }

            return Ok(CustomResponseMessage.OkCustom<string?>("Password changed successfully", null));
        }


        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<ResponseModel<AuthResponse>>> Login([FromBody] UserLoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid request parameters?"));
            }

            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["email"] = loginDto.Email;

            User? existingUser = _userRepository.Get(condition, resolveRelation: true);

            if (existingUser == null)
            {
                return NotFound(CustomResponseMessage.ErrorCustom("Not Found", "No user was found"));
            }
            if (!PasswordManager.VerifyPassword(loginDto.Password, existingUser.Password))
            {
                return Unauthorized(CustomResponseMessage.ErrorCustom("Unauthorized", "Authentication failed"));
            }

            if (existingUser.Status != UserStatus.OK)
            {
                return Unauthorized(CustomResponseMessage.ErrorCustom("Unauthorized", $"Account error. Status: {existingUser.Status}"));
            }
            if (existingUser.DeletedAt != null)
            {
                return Unauthorized(CustomResponseMessage.ErrorCustom("Unauthorized", $"Account error. Deleted by admin"));
            }

            var token = JwtAuthService.GenerateJwtToken(existingUser, _configuration);

            return Ok(CustomResponseMessage.OkCustom("Login successful.", new AuthResponse { Token = token, user = existingUser }));

        }

        [HttpPost]
        [Route("signup")]
        public async Task<ActionResult<ResponseModel<string?>>> Signup([FromBody] UserSignUpDTO signupDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid request parameters?"));
            }

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
                return BadRequest(CustomResponseMessage.ErrorCustom("bad Request", "No user was created"));
            }
            //todo: add email verification process

            return Ok(CustomResponseMessage.OkCustom<string?>("Signup successful.", null));
            //maybe auto login?
            //var token = JwtAuthService.GenerateJwtToken(existingUser, _configuration);
            //return new AuthResponse { Token = token, user = existingUser };
        }


        ///// for social login/signup ////


        [HttpPost]
        [Route("social-auth")]
        public async Task<ActionResult<ResponseModel<object?>>> SocialAuth([FromBody] SocialAuthDTO socialAuthDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid request parameters?"));
            }
            //verify the id token

            GoogleJsonWebSignature.Payload? payload;
            try
            {
                string idToken = socialAuthDto.IdToken;
                string clientId = _configuration["ExternalAPIs:google:web:client_id"];
                payload = await GoogleApiAuth.VerifyIdTokenAsync(idToken, clientId);
            }
            catch (Exception ex)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("bad Request", "Unable to verify user info," + ex.Message));
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
                return Ok(CustomResponseMessage.OkCustom("Login successful.", new AuthResponse { Token = token, user = existingUser }));
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
                return BadRequest(CustomResponseMessage.ErrorCustom("bad Request", "No user was created"));
            }
            user.Id = Convert.ToInt64(createStatus);
            user.Role = new Role { Id = 1, RoleName = "user" };

            var token2 = JwtAuthService.GenerateJwtToken(user, _configuration);
            return Ok(CustomResponseMessage.OkCustom("Signup successful.", new AuthResponse { Token = token2, user = user }));
        }


        [HttpPost]
        [Authorize]
        [Route("logout")]
        public async Task<ActionResult> Logout()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            return Redirect("/");
        }





    }
}
