using Services;
using System.Collections.Concurrent;

namespace ExcelReader.Services.Queues
{
    public class AgentQueue
    {
        //key is userId of agent and value is in-call customer id,agent name
        private static readonly ConcurrentDictionary<string, (int, string)> agentList = new();

        public AgentQueue()
        {

        }

        //get the userId of an agent who is not in call
        public (string, string)? GetOneAvailableAgent()
        {
            return (agentList.Where(item => item.Value.Item1 == 0).FirstOrDefault().Key, agentList.Where(item => item.Value.Item1 == 0).FirstOrDefault().Value.Item2);
        }

        public IEnumerable<string> GetAllAvailableAgent()
        {
            return agentList.Where(item => item.Value.Item1 == 0).Select(it => it.Key);
        }

        public IEnumerable<string> GetAllBusyAgent()
        {
            return agentList.Where(item => item.Value.Item1 != 0).Select(it => it.Key);
        }

        public bool AddNewAgent(string agentId, string agentName, int state = 0)
        {
            var addValue = agentList.AddOrUpdate(agentId, (state, agentName), (k, old) =>
            {
                return (state, agentName);
            });
            return true;
        }

        public bool RemoveAgent(string agentId)
        {
            return agentList.Remove(agentId, out var v);
        }

        public bool FreeAgentFromCall(string agentId)
        {
            //ErrorConsole.Log($"I: Release agent={agentId} from call");

            try
            {
                agentList[agentId] = (0, agentList[agentId].Item2);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool AssignCallToAgent(int CustomerId, out (string, string)? agentId)
        {
            agentId = GetOneAvailableAgent();

            if (CustomerId == 0)
            {
                return false;
            }
            var addValue = agentList.AddOrUpdate(agentId?.Item1, (CustomerId, ""), (k, old) =>
            {
                if (old.Item1 != 0)
                {
                    return old;
                }
                return (CustomerId, old.Item2);
            });
            return addValue.Item1 == CustomerId;
        }
        public bool AssignCallToAgentWithId(int CustomerId, string agentId)
        {
            if (CustomerId == 0)
            {
                return false;
            }
            var addValue = agentList.AddOrUpdate(agentId, (CustomerId, ""), (k, old) =>
            {
                if (old.Item1 != 0)
                {
                    return old;
                }
                return (CustomerId, old.Item2);
            });
            return addValue.Item1 == CustomerId;
        }

        public string GetAgentForUser(int userId, out string? agentName)
        {
            var agent = agentList.Where(item => item.Value.Item1.Equals(userId)).Select(it => (it.Key, it.Value.Item2)).FirstOrDefault();
            agentName = agent.Item2;
            return agent.Item1;
        }

        public int GetUserForAgent(string agentId)
        {
            return agentList.Where(item => item.Key.Equals(agentId)).Select(it => it.Value.Item1).FirstOrDefault();
        }
    }
}