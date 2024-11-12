using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            _userConnections.TryRemove(userId, out _);
            if (isUserAdmin())
            {
                _chatQueueService.TryRemoveByUserId(_chatQueueService.GetUserForAgent(userId.ToString()).ToString());
                _chatQueueService.RemoveAgent(userId);

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
    }
}
