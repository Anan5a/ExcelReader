using BLL;
using DocumentFormat.OpenXml.Presentation;
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
            var configState = AdminBLL.NewUserConfig(_userRepository, _roleRepository, userId);

            if (configState is BLLReturnEnum)
            {
                //no role? something is very wrong
                return StatusCode(StatusCodes.Status403Forbidden, CustomResponseMessage.ErrorCustom("Forbidden", "This action is forbidden at this moment."));
            }

            return Ok(CustomResponseMessage.OkCustom<object>("Successful query", configState));
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
            var createState = AdminBLL.CreateUser(_userRepository, _roleRepository, userId, adminNewUserDto);
            switch (createState)
            {
                case BLLReturnEnum.Admin_ADMIN_NO_ROLE_LIST:
                    // No role? Something is very wrong
                    return StatusCode(StatusCodes.Status403Forbidden, CustomResponseMessage.ErrorCustom("Forbidden", "This action is forbidden at this moment."));

                case BLLReturnEnum.Admin_ADMIN_CREATE_USER_INVALID_ROLE:
                    // Invalid role was provided :/
                    return StatusCode(StatusCodes.Status403Forbidden, CustomResponseMessage.ErrorCustom("Forbidden", "Failed to add user. An invalid role was provided"));

                case BLLReturnEnum.User_USER_NO_USER_CREATED:
                    return BadRequest(CustomResponseMessage.ErrorCustom("bad Request", "No user was created"));

                default:
                    return Ok(CustomResponseMessage.OkCustom<string?>("New user was added.", null));
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin, super_admin")]
        [Route("user-info/{userId}")]
        public async Task<ActionResult<ResponseModel<User?>>> UserInfo(Int32 userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid request parameters?"));
            }

            Dictionary<string, dynamic> condition = new Dictionary<string, dynamic>();

            condition["id"] = userId;
            User? existingUser = _userRepository.Get(condition, resolveRelation: true);


            return Ok(CustomResponseMessage.OkCustom<User?>("Query ok.", existingUser));

        }


    }
}