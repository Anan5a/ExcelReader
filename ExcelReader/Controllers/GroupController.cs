using BLL;
using DataAccess.IRepository;
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

    public class GroupController : Controller
    {
        private IConfiguration _configuration;
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupMembershipRepository _groupMembershipRepository;
        public GroupController(IConfiguration configuration, IGroupRepository groupRepository, IGroupMembershipRepository groupMembershipRepository)
        {
            _groupMembershipRepository = groupMembershipRepository;
            _configuration = configuration;
            _groupRepository = groupRepository;
        }

        [HttpGet]
        [Route("list")]
        public async Task<ActionResult<ResponseModel<IEnumerable<GroupModel>>>> ListGroups([FromQuery] string? page)
        {
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var list = AdminBLL.ListGroups(_groupRepository, null);
                
            return Ok(CustomResponseMessage.OkCustom<IEnumerable<GroupModel>>("Query successful.", list));

        }


        [HttpPost]
        [Route("create-group")]
        public async Task<ActionResult<ResponseModel<bool?>>> CreateGroup(CreateGroupDTO createGroupDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid request parameters?"));
            }

            //create group
            var status = AdminBLL.CreateGroup(_groupRepository, createGroupDTO);
            if (status != BLLReturnEnum.ACTION_OK)
            {
                return UnprocessableEntity(CustomResponseMessage.ErrorCustom("Unprocessable request", "Could not process request. Maybe try again?"));
            }
            return Ok(CustomResponseMessage.OkCustom<bool?>("New group created successfully.", true));

        }


        [HttpPost]
        [Route("create-group-membership")]

        public async Task<ActionResult<ResponseModel<bool?>>> CreateGroupMembership(CreateGroupMembershipDTO createGroupMembershipDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("Bad Request", "Invalid request parameters?"));
            }

            //create group
            var status = AdminBLL.CreateGroupMembership(_groupMembershipRepository, createGroupMembershipDTO);
            if (status != BLLReturnEnum.ACTION_OK)
            {
                return UnprocessableEntity(CustomResponseMessage.ErrorCustom("Unprocessable request", "Could not process request. Maybe try again?"));
            }
            return Ok(CustomResponseMessage.OkCustom<bool?>("New membership created successfully.", true));

        }

    }
}
