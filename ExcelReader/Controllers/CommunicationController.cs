using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Models.DTOs;
using Models.RealtimeMessage;
using Models;
using Services;
using System.Security.Claims;
using Utility;
using IRepository;
using ExcelReader.Realtime;
using DataAccess.IRepository;

namespace ExcelReader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunicationController : ControllerBase
    {
        private readonly ChatQueueService _chatQueueService;
        private IUserRepository _userRepository;
        private IChatHistoryRepository _chatHistoryRepository;
        private IHubContext<SimpleHub> _hubContext;


        public CommunicationController(
            ChatQueueService chatQueueService,
            IHubContext<SimpleHub> hubContext,
            IUserRepository userRepository,
            IChatHistoryRepository chatHistoryRepository
            )
        {
            _chatQueueService = chatQueueService;
            _hubContext = hubContext;
            _userRepository = userRepository;
            _chatHistoryRepository = chatHistoryRepository;
        }
        //// Messaging system ////


        [HttpGet]
        [Route("online-users")]
        [Authorize(Roles = "admin, super_admin")]
        public async Task<ActionResult<ResponseModel<IEnumerable<ChatUserLimitedDTO>>>> GetOnlineUsers()
        {
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var fromUserId);
            var fromUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var onlineUsers = _chatQueueService.GetQueueUserIds().ToList();
            var userDetails = UserBLL.UsersById(_userRepository, onlineUsers);

            //filter list based on user or admin
            //only show non-admin users
            var returnList = from user in userDetails
                             where
                             (
                                user?.Role?.RoleName != UserRoles.Admin &&
                                user?.Role?.RoleName != UserRoles.SuperAdmin
                             )
                             select new ChatUserLimitedDTO
                             {
                                 Id = user.Id,
                                 Name = user.Name,
                                 AgentInfo = int.TryParse(_chatQueueService.GetAgentForUser(Convert.ToInt32(user.Id), out var agentName), out var agentId)
                                                             ? new()
                                                             {
                                                                 Id = agentId,
                                                                 Name = agentName
                                                             }
                                                             : null
                             };

            return Ok(CustomResponseMessage.OkCustom<IEnumerable<ChatUserLimitedDTO>>("Query ok.", returnList));
        }


        [HttpPost]
        [Route("send-message")]
        [Authorize(Roles = "user, admin, super_admin")]
        public async Task<ActionResult<ResponseModel<long?>>> SendMessage(ChatSendMessageDTO chatSendMessageDTO)
        {
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var fromUserId);

            //store chat
            var storedChatId = _chatHistoryRepository.Add(new()
            {
                SenderId = fromUserId,
                Content = chatSendMessageDTO.Message,
                CreatedAt = DateTime.UtcNow,
                ReceiverId = chatSendMessageDTO.To,
            });
            if (storedChatId == 0)
            {
                //store failed
                //log
                ErrorConsole.Log("Failed to store conversation.");
            }
            //send message to user
            await _hubContext.Clients.User(chatSendMessageDTO.To.ToString()).SendAsync("ChatChannel", new List<ChatChannelMessage>{new ChatChannelMessage
            {
                Content = chatSendMessageDTO.Message,
                From = fromUserId,
                MessageId = storedChatId
            }});

            return Ok(CustomResponseMessage.OkCustom<long?>("Message sent.", storedChatId));

        }

        /// <summary>
        /// Retreives the chat history of a user/customer, sends the data via webrtc
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("chat-history/{userId?}")]
        [Authorize(Roles = "user, admin, super_admin")]
        public async Task<ActionResult<ResponseModel<bool>>> GetChatHistory(long? userId)
        {
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentUserId);
            IEnumerable<ChatHistory> messages;
            if (isUserAdmin() && userId != null)
            {
                //only take into account supplied userId
                UserBLL.GetChatHistoryByUserIdAndAgentId(_chatHistoryRepository, (long)userId, out messages, userId);
            }
            else
            {
                UserBLL.GetChatHistoryByUserIdAndAgentId(_chatHistoryRepository, currentUserId, out messages, currentUserId);
            }


            var transformedMessages = from message in messages
                                      select new ChatChannelMessage
                                      {
                                          Content = message.Content,
                                          From = message.SenderId,
                                          MessageId = message.ChatHistoryId,
                                          SentAt = message.CreatedAt
                                      };

            await _hubContext.Clients.User(currentUserId.ToString()).SendAsync("ChatChannel", transformedMessages);

            return Ok(CustomResponseMessage.OkCustom<bool>("Query ok.", true));
        }


        //user enter the queue aka. asking for support
        [HttpPost]
        [Route("enter-chat-queue")]
        [Authorize(Roles = "user")]

        public async Task<ActionResult<ResponseModel<bool>>> EnterChatQueue()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            //we don't want to enqueue admins to call
            if (isUserAdmin())
            {
                return BadRequest(CustomResponseMessage.ErrorCustom("action error", "Agent/Admins cannot enter customer queue."));
            }
            //add user call to queue
            _chatQueueService.Enqueue(new QueueModel
            {
                EntryTime = DateTime.Now,
                UserId = userId,
                Username = userName,
            });
            //further processing is done in background queue processor

            return Ok(CustomResponseMessage.OkCustom("Agent request", true));
        }


        [HttpPost]
        [Route("accept-user-into-chat")]
        [Authorize(Roles = "admin, super_admin")]

        public async Task<ActionResult<ResponseModel<bool>>> AcceptUserIntoChat([FromBody] ChatUserIdOnlyDTO chatUserIdOnlyDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            //try to assign user to agent
            var assignmentStatus = await _chatQueueService.ProcessNextCallWithAgentAndUserAsync(Convert.ToInt32(chatUserIdOnlyDTO.Id), userId);

            if (assignmentStatus != null)
            {

                if (assignmentStatus.Value.Item1)
                {
                    RTC_SendSignalToAgent(assignmentStatus.Value.Item2, userId.ToString());
                    RTC_SendSignalToUser(new ChatUserLimitedDTO
                    {
                        Id = Convert.ToInt64(userId),
                        Name = userName,
                    }, assignmentStatus.Value.Item2.UserId);

                    return Ok(CustomResponseMessage.OkCustom("Accept user into chat successful", true));
                }
                else
                {
                    return Ok(CustomResponseMessage.OkCustom("Accept user into chat failed, try refreshing", false));

                }
            }
            return Ok(CustomResponseMessage.OkCustom("Accept user into chat failed", false));
        }


        [HttpPost]
        [Route("close-chat")]
        [Authorize(Roles = "user, admin, super_admin")]

        public async Task<ActionResult<ResponseModel<bool>>> ExitChatQueue()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            //remove user and free agent 
            if (isUserAdmin())
            {
                _chatQueueService.TryRemoveByUserId(_chatQueueService.GetUserForAgent(userId).ToString());
                _chatQueueService.FreeAgentFromCall(userId);

            }
            else
            {
                var agentIdOfAgentFromUserId = _chatQueueService.GetAgentForUser(Convert.ToInt32(userId), out var _);
                _chatQueueService.TryRemoveByUserId(userId);
                await _hubContext.Clients.User(agentIdOfAgentFromUserId.ToString()).SendAsync("ChatChannel", new List<ChatChannelMessage>{
                    new ChatChannelMessage
                    {
                        Content = $"{userName} left chat",
                        From = Convert.ToInt64(userId),
                        IsSystemMessage = true,
                        SentAt=DateTime.Now,
                        MessageId=0
                    }
             });
            }


            return Ok(CustomResponseMessage.OkCustom("Chat exit ok", true));
        }

        private bool isUserAdmin()
        {
            var userRole = User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            return userRole == "admin" || userRole == "super_admin";
        }





        #region RTCSignaling

        private async void RTC_SendSignalToAgent(QueueModel callQueueModel, string agentId)
        {
            //data fromat==>> <req>:<id>,
            //await _hubContext.Clients.User(agentId).SendAsync("CallingChannel", new CallingChannelMessage
            //{
            //    Metadata = new RTCConnModel
            //    {
            //        Data = null,
            //        TargetUserId = Convert.ToInt32(callQueueModel.UserId),
            //        TargetUserName = callQueueModel.Username,
            //    }
            //});
            await _hubContext.Clients.User(agentId).SendAsync("AgentChannel", new AgentChannelMessage<RTCConnModel>
            {
                CallData = "",
                Metadata = new RTCConnModel
                {
                    Data = null,
                    TargetUserId = Convert.ToInt32(callQueueModel.UserId),
                    TargetUserName = callQueueModel.Username,
                }
            });

        }

        // sends agent info to user after an agent is assigned
        private async void RTC_SendSignalToUser(ChatUserLimitedDTO agentInfo, string userId)
        {
            //data fromat==>> <req>:<id>,
            await _hubContext.Clients.User(userId).SendAsync("AgentChannel", new AgentChannelMessage<RTCConnModel>
            {
                CallData = "",
                Metadata = new RTCConnModel
                {
                    Data = null,
                    TargetUserId = Convert.ToInt32(agentInfo.Id),
                    TargetUserName = agentInfo.Name,
                }
            });
            await _hubContext.Clients.User(userId).SendAsync("ChatChannel", new List<ChatChannelMessage>{ new ChatChannelMessage
            {
                Content = $"{agentInfo.Name} joined chat",
                From = agentInfo.Id,
                IsSystemMessage = true,
                SentAt=DateTime.Now,
                MessageId=0
            } });
        }
        #endregion


    }
}
