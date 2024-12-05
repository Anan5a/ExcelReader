using BLL;
using IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;
using Services;
using System.Security.Claims;
using Utility;

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

        public UserController(
            IWebHostEnvironment webHostEnvironment,
            IUserRepository userRepository,
            IConfiguration configuration,
            IFileMetadataRepository fileMetadataRepository


            )
        {
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
            _configuration = configuration;
            _fileMetadataRepository = fileMetadataRepository;
        }



        [HttpGet]
        [Route("list")]
        [Authorize(Roles = "admin, super_admin")]
        public async Task<ActionResult<ResponseModel<IEnumerable<User>>>> UsersList()
        {

            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            var users = _userRepository.GetAll(null, resolveRelation: true);
            //we don't want to return the currently logged in user
            var filtered = users.Where(user => user.UserId != userId);
            return Ok(CustomResponseMessage.OkCustom<IEnumerable<User>>("Successful query", users));
        }

        [HttpGet]
        [Route("dashboard")]
        [Authorize(Roles = "user, admin, super_admin")]
        public async Task<ActionResult<ResponseModel<object>>> Dashboard()
        {

            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);

            var userFileCount = UserBLL.Dashboard(_fileMetadataRepository, userId);

            return Ok(CustomResponseMessage.OkCustom<object>("Successful query", new { fileCount = userFileCount }));
        }


        [HttpPost]
        [Route("change-password")]
        [Authorize(Roles = "user, admin, super_admin")]
        public async Task<ActionResult<ResponseModel<string?>>> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {

            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);


            var status = UserBLL.ChangePassword(_userRepository, userId, changePasswordDTO);

            switch (status)
            {
                case BLLReturnEnum.ACTION_OK:
                    return Ok(CustomResponseMessage.OkCustom<string?>("Password changed successfully", null));
                case BLLReturnEnum.User_USER_NOT_FOUND:
                    return NotFound(CustomResponseMessage.ErrorCustom("Not found", "User was not found!"));

                case BLLReturnEnum.User_USER_AUTH_FAILED:
                    return StatusCode(StatusCodes.Status403Forbidden, CustomResponseMessage.ErrorCustom("Forbidden", "Failed to verify old password."));

                case BLLReturnEnum.ACTION_ERROR:
                    return BadRequest(CustomResponseMessage.ErrorCustom("update error", "Failed to update password. Try again later."));

                default:
                    return StatusCode(StatusCodes.Status500InternalServerError, CustomResponseMessage.ErrorCustom("Internal Server Error", "An unexpected error occurred."));
            }

        }


        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<ResponseModel<AuthResponse>>> Login([FromBody] UserLoginDTO loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid request parameters?"));
            }

            var loginState = UserBLL.Login(_configuration, _userRepository, loginDto);

            if (loginState is AuthResponse)
            {
                //login was successful
                return Ok(CustomResponseMessage.OkCustom("Login successful.", loginState));
            }

            switch (loginState)
            {
                case BLLReturnEnum.User_USER_NOT_FOUND:
                    return NotFound(CustomResponseMessage.ErrorCustom("Not Found", "No user was found"));
                case BLLReturnEnum.User_USER_AUTH_FAILED:
                    return Unauthorized(CustomResponseMessage.ErrorCustom("Unauthorized", "Authentication failed"));
                case BLLReturnEnum.User_USER_ACCOUNT_DELETED:
                    return Unauthorized(CustomResponseMessage.ErrorCustom("Unauthorized", $"Account error. Deleted by admin"));
                case BLLReturnEnum.User_USER_ACCOUNT_ERROR:
                    return Unauthorized(CustomResponseMessage.ErrorCustom("Unauthorized", $"Account status error."));
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError, CustomResponseMessage.ErrorCustom("Internal Server Error", "An unexpected error occurred."));
            }

        }

        [HttpPost]
        [Route("signup")]
        public async Task<ActionResult<ResponseModel<string?>>> Signup([FromBody] UserSignUpDTO signupDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid request parameters?"));
            }

            var createStatus = UserBLL.Signup(_userRepository, signupDto);
            if (createStatus == BLLReturnEnum.ACTION_ERROR)
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

            var userState = UserBLL.SocialAuth(_configuration, _userRepository, socialAuthDto);

            if (userState is AuthResponse)
            {
                return Ok(CustomResponseMessage.OkCustom("Action successful.", userState));
            }


            if (userState is BLLReturnEnum)
            {
                if (userState == BLLReturnEnum.ACTION_ERROR)
                {

                    return BadRequest(CustomResponseMessage.ErrorCustom("bad Request", "No user was created"));
                }
            }
            return BadRequest(CustomResponseMessage.ErrorCustom("bad Request", "No user was created"));
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