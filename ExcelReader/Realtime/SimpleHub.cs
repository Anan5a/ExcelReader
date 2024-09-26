using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ExcelReader.Realtime
{
    [Authorize(Roles = "admin,user,super_admin")]
    public class SimpleHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();

        public override Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            _userConnections.TryAdd(userId, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            _userConnections.TryRemove(userId, out _);
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


    }
}
