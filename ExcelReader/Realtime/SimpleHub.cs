using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Models.RealtimeMessage;
using Services;
using System.Collections.Concurrent;
using System.Security.Claims;
using Utility;

namespace ExcelReader.Realtime
{
    [Authorize(Roles = "admin,user,super_admin")]
    public class SimpleHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();
        private readonly ChatQueueService _chatQueueService;
        public static string GroupNameKey = "AgentGroup";

        public SimpleHub(ChatQueueService chatQueueService)
        {
            _chatQueueService = chatQueueService;
        }
        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var userName = Context?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            _userConnections.TryAdd(userId, Context.ConnectionId);

            if (isUserAdmin())
            {
                //add to agent list
                _chatQueueService.AddNewAgent(userId, userName);
                JoinAgentGroup();
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            _userConnections.TryRemove(userId, out _);
            if (isUserAdmin())
            {
                var userIdOfUserFromAgent = _chatQueueService.GetUserForAgent(userId.ToString());
                _chatQueueService.TryRemoveByUserId(userIdOfUserFromAgent.ToString());
                _chatQueueService.RemoveAgent(userId);

                ExitAgentGroup();
                //send notification to user that admin left chat
                Clients.User(userIdOfUserFromAgent.ToString()).SendAsync("ChatChannel", new List<ChatChannelMessage>{
                    new ChatChannelMessage
                    {
                        EndOfChatMarker=true,
                        Content = "Agent left chat",
                        From = Convert.ToInt64(userId),
                        IsSystemMessage = true,
                        SentAt=DateTime.Now,
                        MessageId=0
                    }
             });
            }
            else
            {
                //remove user from call queue when disconnected
                _chatQueueService.FreeAgentFromCall(_chatQueueService.GetAgentForUser(Convert.ToInt32(userId), out _));
                _chatQueueService.TryRemoveByUserId(userId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("PublicChannel", message);
        }

        public static List<long> GetConnectedUserList()
        {
            return _userConnections.Keys.Select(long.Parse).ToList();
        }

        private bool isUserAdmin()
        {
            var userRole = Context?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            return userRole == UserRoles.Admin || userRole == UserRoles.SuperAdmin;
        }

        private async void JoinAgentGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupNameKey);

        }
        private async void ExitAgentGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNameKey);

        }
    }
}