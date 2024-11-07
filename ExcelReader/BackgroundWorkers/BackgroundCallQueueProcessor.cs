
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelReader.Queues;
using ExcelReader.Realtime;
using Microsoft.AspNetCore.SignalR;
using Models;
using Models.RealtimeMessage;
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

                if (availableAgents.Count() > 0 && _callQueue.Count > 0)
                {

                    //dequeue
                    var dequeueItem = await _callQueue.DequeueAsync();
                    //assign agent
                    if (agentQueue.AssignCallToAgent(int.Parse(dequeueItem.UserId), out var agentId))
                    {
                        //assignment successful
                        //start call establishment process, somehow
                        ErrorConsole.Log($"  Assign call:: qSize={_callQueue.Count} :: from={dequeueItem.UserId} -> to={agentId}\n\t{dequeueItem.ToString()}");
                        RTC_SendSignalToAgent(dequeueItem, agentId);
                    }
                    else
                    {
                        ErrorConsole.Log($"  Assign call:: qSize={_callQueue.Count} :: FAIL");

                    }


                }
                else
                {
                    //delay to prevent wasting cpu cycles when we have no agents
                    await Task.Delay(1000);
                }
                //send notification to agent
                ErrorConsole.Log("End call processing...");
            }
        }



        #region RTCSignaling

        private async void RTC_SendSignalToAgent(CallQueueModel callQueueModel, string agentId)
        {
            //data fromat==>> <req>:<id>,
            await _hubContext.Clients.User(agentId).SendAsync("CallingChannel", new CallingChannelMessage
            {
                CallData = callQueueModel.CallId,
                Metadata = new RTCConnModel
                {
                    Data = null,
                    TargetUserId = Convert.ToInt32(callQueueModel.UserId),
                    TargetUserName = callQueueModel.Username,
                }
            });

        }
        #endregion
    }
}
