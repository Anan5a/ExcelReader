using ExcelReader.Queues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace ExcelReader.Realtime
{
    [Authorize(Roles = "admin,user,super_admin")]
    public class SimpleHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();
        private readonly AgentQueue agentQueue;
        public SimpleHub(AgentQueue agentQueue)
        {
            this.agentQueue = agentQueue;
        }
        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            _userConnections.TryAdd(userId, Context.ConnectionId);

            if (isUserAdmin())
            {
                //add to agent list
                agentQueue.AddNewAgent(userId);
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            _userConnections.TryRemove(userId, out _);
            if (isUserAdmin())
            {
                agentQueue.RemoveAgent(userId);
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
            return userRole == "admin"|| userRole == "super_admin";
        }
    }
}
