using DataAccess.IRepository;
using ExcelReader.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExcelReader.Services;
using ExcelReader.Utility;
using ExcelReader.Models;
using ExcelReader.Models.DTOs;

namespace ExcelReader.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class UserController : Controller
    {
        private IWebHostEnvironment _webHostEnvironment;
        private IUserRepository _userRepository;
        private IConfiguration _configuration;

        public UserController(IWebHostEnvironment webHostEnvironment, IUserRepository userRepository, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
            _configuration = configuration;

        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] UserLoginDTO loginDto)
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
            return new AuthResponse { Token = token, user = existingUser };

        }

        [HttpPost]
        [Route("signup")]
        public async Task<ActionResult<>> Login([FromBody] UserSignUpDTO signupDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid request parameters?"));
            }

            User user = new User
            {
                Email = signupDto.Email,
                Name = signupDto.Name,
                Password = signupDto.Password,
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

            return Ok(CustomResponseMessage.OkCustom("Signup successful."));
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
