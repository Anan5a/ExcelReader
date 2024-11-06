using System.Collections.Concurrent;

namespace ExcelReader.Queues
{
    public class AgentQueue
    {
        //key is userId of agent and value is in-call customer id
        private static readonly ConcurrentDictionary<string, int> agentList = new();

        public AgentQueue()
        {

        }

        //get the userId of an agent who is not in call
        public string GetOneAvailableAgent()
        {
            return agentList.Where(item => item.Value == 0).FirstOrDefault().Key;
        }
        public IEnumerable<string> GetAllAvailableAgent()
        {
            return agentList.Where(item => item.Value == 0).Select(it => it.Key);
        }
        public IEnumerable<string> GetAllBusyAgent()
        {
            return agentList.Where(item => item.Value != 0).Select(it => it.Key);
        }

        public bool AddNewAgent(string agentId, int state = 0)
        {
            return agentList.TryAdd(agentId, state);
        }
        public bool RemoveAgent(string agentId)
        {
            return agentList.Remove(agentId, out int v);
        }
        public bool FreeAgentFromCall(string agentId)
        {
            try
            {
                agentList[agentId] = 0;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public bool AssignCallToAgent(int CustomerId, out string agentId)
        {
            agentId = GetOneAvailableAgent();

            if (CustomerId == 0)
            {
                return false;
            }
            var addValue = agentList.AddOrUpdate(agentId, CustomerId, (k, old) =>
            {
                if (old != 0)
                {
                    return old;
                }
                return CustomerId;
            });
            return addValue == CustomerId;
        }

    }
}
