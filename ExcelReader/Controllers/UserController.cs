using DataAccess.IRepository;
using ExcelReader.DataAccess.IRepository;
using ExcelReader.Models;
using ExcelReader.Models.DTOs;
using ExcelReader.Services;
using ExcelReader.Utility;
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
            //if (User.Identity != null && !User.Identity.IsAuthenticated)
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
                Status = UserStatus.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = null,
                DeletedAt = null,
                VerifiedAt = null,
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
