using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Threading;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;
using System.Net;
using System.Configuration;

namespace VisionEdgeWebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private static string connectionString;
        private static string eventHubName;
        public static ServiceClient iotHubServiceClient { get; private set; }
        public static EventHubClient eventHubClient { get; private set; }
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var host = CreateEventProcessorHost();
            // The following code ensures that the WebJob will be running continuously
            Trace.TraceInformation("EventsForwarding OnStart()...\n");
            host.RegisterEventProcessorAsync<SensorEventProcessor>().Wait();

            Trace.TraceInformation("Receiving events...\n");
            Console.WriteLine("Press any key to stop processing...");
            Console.ReadLine();

            host.UnregisterEventProcessorAsync().Wait();
        }
        public static EventProcessorHost CreateEventProcessorHost()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            eventHubName = ConfigurationManager.AppSettings["Microsoft.ServiceBus.EventHubName"];
            string storageAccountName = ConfigurationManager.AppSettings["AzureStorage.AccountName"];
            string storageAccountKey = ConfigurationManager.AppSettings["AzureStorage.Key"];
            string storageAccountString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                storageAccountName, storageAccountKey);
            string iotHubConnectionString = ConfigurationManager.AppSettings["AzureIoTHub.ConnectionString"];
            iotHubServiceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);

            var defaultConsumerGroup = eventHubClient.GetDefaultConsumerGroup();

            string eventProcessorHostName = "SensorEventProcessor";
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, defaultConsumerGroup.GroupName, connectionString, storageAccountString);
            
            return eventProcessorHost;
        }
    }
}
