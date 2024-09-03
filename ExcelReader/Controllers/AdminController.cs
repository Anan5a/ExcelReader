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
    [Authorize(Roles = "admin, super_admin")]
    public class AdminController : Controller
    {

        private IWebHostEnvironment _webHostEnvironment;
        private IConfiguration _configuration;
        private IUserRepository _userRepository;
        private readonly IFileMetadataRepository _fileMetadataRepository;
        private readonly IRoleRepository _roleRepository;

        public AdminController(IWebHostEnvironment webHostEnvironment, IUserRepository userRepository, IConfiguration configuration, IFileMetadataRepository fileMetadataRepository, IRoleRepository roleRepository)
        {
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
            _configuration = configuration;
            _fileMetadataRepository = fileMetadataRepository;
            _roleRepository = roleRepository;
        }



        [HttpGet]
        [Route("new-user-config")]
        public async Task<ActionResult<ResponseModel<object>>> NewUserConfig()
        {
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            IEnumerable<Role> assignableRoles = GetAssignableRoles(userId);
            if (assignableRoles.Count() == 0)
            {
                //no role? something is very wrong
                return StatusCode(StatusCodes.Status403Forbidden, CustomResponseMessage.ErrorCustom("Forbidden", "This action is forbidden at this moment."));
            }

            return Ok(CustomResponseMessage.OkCustom<object>("Successful query", new
            {
                roles = assignableRoles
            }));
        }

        [HttpPost]
        [Route("create-user")]
        public async Task<ActionResult<ResponseModel<string?>>> CreateUser([FromBody] AdminNewUserDTO adminNewUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid request parameters?"));
            }
            //verify role validity
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            IEnumerable<Role> assignableRoles = GetAssignableRoles(userId);

            //find user and user files
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["id"] = userId;


            if (assignableRoles.Count() == 0)
            {
                //no role? something is very wrong
                return StatusCode(StatusCodes.Status403Forbidden, CustomResponseMessage.ErrorCustom("Forbidden", "This action is forbidden at this moment."));

            }

            if (assignableRoles.FirstOrDefault(role => role.Id == adminNewUserDto.RoleId, null) == null)
            {
                //invalid role was provided :/
                return StatusCode(StatusCodes.Status403Forbidden, CustomResponseMessage.ErrorCustom("Forbidden", "Failed to add user. An invalid role was provided"));

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
                return BadRequest(CustomResponseMessage.ErrorCustom("bad Request", "No user was created"));
            }
            //todo: add email verification process

            return Ok(CustomResponseMessage.OkCustom<string?>("New user was added.", null));


        }




        private IEnumerable<Role>? GetAssignableRoles(long userId)
        {
            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();
            condition["id"] = userId;

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
    }
}
