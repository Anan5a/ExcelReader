
using ExcelReader.Queues;
using ExcelReader.Realtime;
using Microsoft.AspNetCore.SignalR;
using Models;
using Services;

namespace ExcelReader.BackgroundWorkers
{
    public class BackgroundCallQueueProcessor : BackgroundService
    {
        private readonly ICallQueue<CallQueueModel> _callQueue;
        private readonly AgentQueue agentQueue;
        private readonly IHubContext<SimpleHub> _hubContext;

        public BackgroundCallQueueProcessor(
            ICallQueue<CallQueueModel> callQueue,
            AgentQueue agentQueue,
            IHubContext<SimpleHub> hubContext
)
        {
            _callQueue = callQueue;
            this.agentQueue = agentQueue;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ErrorConsole.Log("I: ..:::Start Processing call queue:::.. ");

            while (!stoppingToken.IsCancellationRequested)
            {
                //var queueItem = await _callQueue.PeekAsync();
                var availableAgents = agentQueue.GetAllAvailableAgent();
                ErrorConsole.Log($"  Agents:: n={availableAgents.Count()}  : [" + string.Join(",", availableAgents.ToList()) + "]");

                if (availableAgents.Count() > 0)
                {

                    //dequeue
                    var dequeueItem = await _callQueue.DequeueAsync();
                    //assign agent
                    if (agentQueue.AssignCallToAgent(int.Parse(dequeueItem.UserId), out var agentId))
                    {
                        //assignment successful
                        //start call establishment process, somehow
                        ErrorConsole.Log($"  Assign call:: qSize={_callQueue.Count} :: from={dequeueItem.UserId} -> to={agentId}");
                    }
                    else
                    {
                        ErrorConsole.Log($"  Assign call:: qSize={_callQueue.Count} :: FAIL");

                    }


                }
                ErrorConsole.Log("End call processing...");
                //send notification to agent
                await Task.Delay(800);
            }
        }
    }
}
