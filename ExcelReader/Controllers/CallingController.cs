using ExcelReader.Queues;
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
        private readonly ICallQueue<CallQueueModel> _callQueue;

        private IHubContext<SimpleHub> _hubContext;
        public CallingController(
           IWebHostEnvironment webHostEnvironment,
           IUserRepository userRepository,
           IFileMetadataRepository fileMetadataRepository,
           IConfiguration configuration,
           IHubContext<SimpleHub> hubContext,
           ICallQueue<CallQueueModel> callQueue)
        {
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
            _fileMetadataRepository = fileMetadataRepository;
            _configuration = configuration;
            _hubContext = hubContext;
            _callQueue = callQueue;
        }



        //[HttpPost]
        //[Route("queueCall")]
        //[AllowAnonymous]

        //public async Task<ActionResult<ResponseModel<bool>>> QueueCall([FromBody] string s1)
        //{
        //    _callQueue.Enqueue(s1);

        //    return Ok(CustomResponseMessage.OkCustom($"Queued: {s1}", true));

        //}



        [HttpPost]
        [Route("offerCallRequest")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<bool>>> OfferCallRequest([FromBody] RTCConnModel rtcConnRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            //add user call to queue

            _callQueue.Enqueue(new CallQueueModel
            {
                CallId = rtcConnRequest.Data,
                CallTime = DateTime.Now,
                UserId = userId,
                Username = userName,
            });
            //further processing is done in background queue processor

            return Ok(CustomResponseMessage.OkCustom("Call Offer sent", true));







            var target = rtcConnRequest.TargetUserId;

            rtcConnRequest.TargetUserId = Convert.ToInt32(userId);
            rtcConnRequest.TargetUserName = userName!;
            //data fromat==>> <req>:<id>,
            var data = rtcConnRequest.Data;
            rtcConnRequest.Data = "";
            await _hubContext.Clients.User(target.ToString()).SendAsync("CallingChannel", new CallingChannelMessage
            {
                CallData = data,
                Metadata = rtcConnRequest
            });

        }

        [HttpPost]
        [Route("offerCallRequestAnswer")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<bool>>> OfferCallRequestAnswer([FromBody] RTCConnModel rtcConnRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            var target = rtcConnRequest.TargetUserId;

            rtcConnRequest.TargetUserId = Convert.ToInt32(userId);
            rtcConnRequest.TargetUserName = userName!;
            //data fromat==>> <req>:<id>,
            var data = rtcConnRequest.Data;
            rtcConnRequest.Data = "";
            await _hubContext.Clients.User(target.ToString()).SendAsync("CallingChannel", new CallingChannelMessage
            {
                CallData = data,
                Metadata = rtcConnRequest
            });
            return Ok(CustomResponseMessage.OkCustom("Call Offer answer sent", true));

        }

        [HttpPost]
        [Route("offerCall")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<bool>>> RTCOfferCall([FromBody] RTCConnModel rtcConnRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            var target = rtcConnRequest.TargetUserId;

            rtcConnRequest.TargetUserId = Convert.ToInt32(userId);
            rtcConnRequest.TargetUserName = userName!;

            var rtcData = rtcConnRequest.Data;
            rtcConnRequest.Data = "";
            await _hubContext.Clients.User(target.ToString()).SendAsync("CallingChannel", new CallingChannelMessage
            {
                CallData = rtcData,
                Metadata = rtcConnRequest
            });
            return Ok(CustomResponseMessage.OkCustom("RTC Offer sent", true));

        }

        [HttpPost]
        [Route("answerCall")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<bool>>> RTCAnswerCall([FromBody] RTCConnModel rtcConnRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            rtcConnRequest.TargetUserName = userName;
            var target = rtcConnRequest.TargetUserId;

            rtcConnRequest.TargetUserId = Convert.ToInt32(userId);
            var rtcData = rtcConnRequest.Data;
            rtcConnRequest.Data = "";
            await _hubContext.Clients.User(target.ToString()).SendAsync("CallingChannel", new CallingChannelMessage
            {
                CallData = rtcData,
                Metadata = rtcConnRequest
            });
            return Ok(CustomResponseMessage.OkCustom("RTC Answer sent", true));
        }


        [HttpPost]
        [Route("sendICECandidate")]
        [Authorize(Roles = "user, admin, super_admin")]
        public async Task<ActionResult<ResponseModel<bool>>> RTCSendICECandidate([FromBody] RTCConnModel rtcConnRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId);
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            var target = rtcConnRequest.TargetUserId;
            rtcConnRequest.TargetUserId = Convert.ToInt32(userId);
            rtcConnRequest.TargetUserName = userName;
            var rtcData = rtcConnRequest.Data;
            rtcConnRequest.Data = "";
            await _hubContext.Clients.User(target.ToString()).SendAsync("CallingChannel", new CallingChannelMessage
            {
                CallData = rtcData,
                Metadata = rtcConnRequest,
                Message = "ICE Data from remote"
            });
            return Ok(CustomResponseMessage.OkCustom("RTC ICE sent", true));
        }


    }
}
