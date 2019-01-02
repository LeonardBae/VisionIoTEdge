using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Microsoft.Azure.Devices;
using System.Net.Http;
using System.Configuration;

namespace VisionEdgeWebJob
{
    class SensorEventProcessor : IEventProcessor
    {
        private static HttpClient client = new HttpClient();
        public static string hubmessage = null;
        public static RegistryManager registryManager = RegistryManager.CreateFromConnectionString(ConfigurationManager.AppSettings["AzureIoTHub.ConnectionString"]);
        public static int[] flag = new int[5];

        Stopwatch checkpointStopWatch;
        PartitionContext partitionContext;
        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation(string.Format("EventProcessor Shutting Down. Partition '{0}', Reason: '{1}'.",
                this.partitionContext.Lease.PartitionId, reason.ToString()));
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        public Task OpenAsync(PartitionContext context)
        {
            Trace.TraceInformation(string.Format("Initializing EventProcessor: Partitioin: '{0}', Offset: '{1}'",
                context.Lease.PartitionId, context.Lease.Offset));
            this.partitionContext = context;
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Trace.TraceInformation("\n");
            Trace.TraceInformation("........ProcessEventsAsync........");
            foreach (EventData eventData in messages)
            {
                try
                {
                    hubmessage = Encoding.UTF8.GetString(eventData.GetBytes());
                    List<SensorEvent> edgedata = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SensorEvent>>(hubmessage);

                    for (int i = 0; i < 5; i++)
                    {
                        if (edgedata[i].Probability > 0.9)
                        {
                            AddTagsAndQuery(edgedata[i].Tag).Wait();
                            flag[i] = 1;
                        }
                        else
                        {
                            flag[i] = 0;
                        }
                    }
                    //
                    //deviceClient = DeviceClient.CreateFromConnectionString("HostName=japanholhub.azure-devices.net;DeviceId=diddevice;SharedAccessKey=e9adjo4n+oFDx5emd6a/xyPjKr7LwZKQ6y89YfvHGNo=");

                    //string jsonString = Encoding.UTF8.GetString(eventData.GetBytes());

                    //Trace.TraceInformation(string.Format("Message received at '{0}'. Partition: '{1}'",
                    //    eventData.EnqueuedTimeUtc.ToLocalTime(), this.partitionContext.Lease.PartitionId));

                    //Trace.TraceInformation(string.Format("-->Raw Data: '{0}'", jsonString));

                    //SensorEvent newSensorEvent = this.DeserializeEventData(jsonString);

                    //Trace.TraceInformation(string.Format("-->Serialized Data: '{0}', '{1}'", newSensorEvent.Tag, newSensorEvent.Probability));

                 
                }
                catch (Exception ex)
                {
                    Trace.TraceInformation("Error in ProssEventsAsync -- {0}\n", ex.Message);
                }
            }

            await context.CheckpointAsync();
        }
        public static async Task AddTagsAndQuery(string name)
        {
            var twin = await registryManager.GetTwinAsync("visionedgehub");
            var patch = "";

            if (name == "chocosongi")
                patch = @"{tags:{choco: 1, gong: 0, go: 0, kan: 0, kanmil: 0}}";
            else if (name == "gongryongbaksa")
                patch = @"{tags:{choco: 0, gong: 1, go: 0, kan: 0, kanmil: 0}}";
            else if (name == "goraebab")
                patch = @"{tags:{choco: 0, gong: 0, go: 1, kan: 0, kanmil: 0}}";
            else if (name == "kancho")
                patch = @"{tags:{choco: 0, gong: 0, go: 0, kan: 1, kanmil: 0}}";
            else if (name == "kanchosweet")
                patch = @"{tags:{choco: 0, gong: 0, go: 0, kan: 0, kanmil: 1}}";

            await registryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);
        }
        private SensorEvent DeserializeEventData(string eventDataString)
        {
            return JsonConvert.DeserializeObject<SensorEvent>(eventDataString);
        }
    }
}
