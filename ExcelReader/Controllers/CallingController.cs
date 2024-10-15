using ExcelReader.Realtime;
using IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Models;
using Models.RealtimeMessage;
using Services;
using System.Security.Claims;

namespace ExcelReader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin,user,super_admin")]

    public class CallingController : Controller
    {

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserRepository _userRepository;
        private readonly IFileMetadataRepository _fileMetadataRepository;
        private readonly IConfiguration _configuration;

        private IHubContext<SimpleHub> _hubContext;
        public CallingController(
           IWebHostEnvironment webHostEnvironment,
           IUserRepository userRepository,
           IFileMetadataRepository fileMetadataRepository,
           IConfiguration configuration,
           IHubContext<SimpleHub> hubContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
            _fileMetadataRepository = fileMetadataRepository;
            _configuration = configuration;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("offerCall")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<bool>>> OfferCall([FromBody] RTCConnModel rtcConnRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            var target = rtcConnRequest.TargetUserId;

            rtcConnRequest.TargetUserId = Convert.ToInt32(userId);
            var rtcData = rtcConnRequest.Data;
            rtcConnRequest.Data = "";
            await _hubContext.Clients.User(target.ToString()).SendAsync("CallingChannel", new CallingChannelMessage
            {
                CallData = rtcData,
                Metadata = rtcConnRequest
            });
            return Ok(CustomResponseMessage.OkCustom("Offer sent", true));

        }

        [HttpPost]
        [Route("answerCall")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<bool>>> AnswerCall([FromBody] RTCConnModel rtcConnRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            var target = rtcConnRequest.TargetUserId;

            rtcConnRequest.TargetUserId = Convert.ToInt32(userId);
            var rtcData = rtcConnRequest.Data;
            rtcConnRequest.Data = "";
            await _hubContext.Clients.User(target.ToString()).SendAsync("CallingChannel", new CallingChannelMessage
            {
                CallData = rtcData,
                Metadata = rtcConnRequest
            });
            return Ok(CustomResponseMessage.OkCustom("Answer sent", true));
        }
        [HttpPost]
        [Route("sendICECandidate")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<bool>>> SendICECandidate([FromBody] RTCConnModel rtcConnRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            var target = rtcConnRequest.TargetUserId;
            rtcConnRequest.TargetUserId = Convert.ToInt32(userId);
            var rtcData = rtcConnRequest.Data;
            rtcConnRequest.Data = "";
            await _hubContext.Clients.User(target.ToString()).SendAsync("CallingChannel", new CallingChannelMessage
            {
                CallData = rtcData,
                Metadata = rtcConnRequest
            });
            return Ok(CustomResponseMessage.OkCustom("Offer sent", true));
        }


    }
}
