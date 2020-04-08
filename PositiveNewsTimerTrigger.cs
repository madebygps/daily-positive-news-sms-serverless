using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MadeByGPS.Function
{
    public static class PositiveNewsTimerTrigger
    {
        [FunctionName("PositiveNewsTimerTrigger")]
        public static void Run([TimerTrigger("0 30 6 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
