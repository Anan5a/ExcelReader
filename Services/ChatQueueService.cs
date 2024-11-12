using ExcelReader.Services.Queues;
using Models;

namespace Services
{
    /// <summary>
    /// This service should contain all logic related to queue management
    /// </summary>
    public class ChatQueueService
    {
        private readonly ICallQueue<QueueModel> _callQueue;
        private readonly AgentQueue _agentQueue;
        public ChatQueueService(ICallQueue<QueueModel> callQueue, AgentQueue agentQueue)
        {
            _callQueue = callQueue;
            _agentQueue = agentQueue;
        }

        /// <summary>
        /// Automatically assign user to an agent
        /// </summary>
        /// <returns></returns>
        public async Task<QueueModel?> ProcessNextCallAsync()
        {
            var availableAgent = _agentQueue.GetOneAvailableAgent();
            if (availableAgent != null)
            {
                var nextCall = await _callQueue.DequeueAsync();
                if (nextCall != null)
                {
                    _agentQueue.AssignCallToAgent(int.Parse(nextCall.UserId), out var agentId);
                    return nextCall;
                }
            }

            return null;
        }

        public bool AddAgent(string userId, string userName)
        {
            return _agentQueue.AddNewAgent(userId, userName);
        }
        public bool AddNewAgent(string agentId, string agentName, int state = 0)
        {
            return _agentQueue.AddNewAgent(agentId, agentName, state);
        }
        public bool RemoveAgent(string userId)
        {
            return _agentQueue.RemoveAgent(userId);
        }
        public string GetAgentForUser(int userId, out string? _)
        {
            return _agentQueue.GetAgentForUser(userId, out _);
        }
        public int GetUserForAgent(string agentId)
        {
            return _agentQueue.GetUserForAgent(agentId);
        }
        public void FreeAgentFromCall(string userId)
        {
            _agentQueue.FreeAgentFromCall(userId);
        }
        public void Enqueue(QueueModel model)
        {
            _callQueue.Enqueue(model);
        }
        public bool TryRemoveByUserId(string item)
        {
            return _callQueue.TryRemoveByUserId(item);
        }
    }
}
