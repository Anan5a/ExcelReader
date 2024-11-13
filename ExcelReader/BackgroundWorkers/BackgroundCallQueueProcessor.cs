using ExcelReader.Services.Queues;
using ExcelReader.Realtime;
using Microsoft.AspNetCore.SignalR;
using Models;
using Models.DTOs;
using Models.RealtimeMessage;
using Services;

namespace ExcelReader.BackgroundWorkers
{
    public class BackgroundCallQueueProcessor : BackgroundService
    {
        private readonly ICallQueue<QueueModel> _customerQueue;
        private readonly AgentQueue agentQueue;
        private readonly IHubContext<SimpleHub> _hubContext;

        public BackgroundCallQueueProcessor(
            ICallQueue<QueueModel> callQueue,
            AgentQueue agentQueue,
            IHubContext<SimpleHub> hubContext
)
        {
            _customerQueue = callQueue;
            this.agentQueue = agentQueue;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //ErrorConsole.Log("I: ..:::Start Processing call queue:::.. ");
            ErrorConsole.Log("I: Automatic assignment is disabled! Stopping background processsing... ");
            return;

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    //var queueItem = await _callQueue.PeekAsync();
            //    var availableAgents = agentQueue.GetAllAvailableAgent();
            //    //ErrorConsole.Log($"  Agents:: n={availableAgents.Count()}  : [" + string.Join(",", availableAgents.ToList()) + "]");
            //    //ErrorConsole.Log($"  Queue:: qn={_customerQueue.Count}");
            //    if (availableAgents.Count() > 0 && _customerQueue.Count > 0)
            //    {

            //        //dequeue
            //        var dequeueItem = await _customerQueue.DequeueAsync();
            //        //assign agent
            //        if (agentQueue.AssignCallToAgent(int.Parse(dequeueItem.UserId), out var agentId))
            //        {
            //            //assignment successful
            //            //start call establishment process, somehow
            //            //ErrorConsole.Log($"  Assign call:: qSize={_customerQueue.Count} :: from={dequeueItem.UserId} -> to={agentId}\n\t{dequeueItem.ToString()}");
            //            RTC_SendSignalToAgent(dequeueItem, agentId?.Item1);
            //            RTC_SendSignalToUser(new ChatUserLimitedDTO
            //            {
            //                Id = Convert.ToInt64(agentId?.Item1),
            //                Name = agentId?.Item2
            //            }, dequeueItem.UserId);
            //        }
            //        else
            //        {
            //            //ErrorConsole.Log($"  Assign call:: qSize={_customerQueue.Count} :: FAIL");

            //        }


            //    }
            //    else
            //    {
            //        //delay to prevent wasting cpu cycles when we have no agents
            //        await Task.Delay(1000);
            //    }
            //    //send notification to agent
            //}
            //ErrorConsole.Log("End call processing...");
        }



    }
}
