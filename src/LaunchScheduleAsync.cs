using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LaunchSchedule.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace LaunchSchedule.Functions
{
    public static class LaunchScheduleAsync
    {
        [FunctionName("CreateLaunchScheduleAsyncV1")]
        public static async Task<HttpResponseMessage> CreateLaunchScheduleAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post",
                            Route = "v1/launch/schedules/async")]LaunchScheduleModel schedule,
            HttpRequestMessage req,
            [Queue("ScheduleCreateRequests", Connection = "ScheduleStorageAccount")]CloudQueue scheduleQueue,
            TraceWriter log)
        {

            //validate model
            ValidationContext validationContext = new ValidationContext(schedule);
            ICollection<ValidationResult> results = new List<ValidationResult>();
            var valid = Validator.TryValidateObject(schedule, validationContext, results, true);

            if (!valid)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Data");
            }

            //Queue Up our insert request with our ID (so the client can have the key to check later on status)
            var id = Guid.NewGuid().ToString();
            schedule.RowKey = id;

            var scheduleJson = JsonConvert.SerializeObject(schedule);
            CloudQueueMessage message = new CloudQueueMessage(scheduleJson);

            await scheduleQueue.AddMessageAsync(message);

            //return http result
            var locationUrl = $"{req.RequestUri.GetLeftPart(UriPartial.Authority)}/api/launch/schedules/{id}";

            var result = req.CreateResponse(HttpStatusCode.Accepted);
            result.Headers.Add("location", locationUrl);

            return result;

        }


        [FunctionName("UpdateLaunchScheduleAsyncV1")]
        public static async Task<HttpResponseMessage> UpdateLaunchScheduleAsync(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put",
                        Route = "v1/launch/schedules/async")]LaunchScheduleModel schedule,
           [Queue("ScheduleUpdateRequests", Connection = "ScheduleStorageAccount")]CloudQueue scheduleQueue,
           HttpRequestMessage req,
           TraceWriter log)
        {
            var scheduleJson = JsonConvert.SerializeObject(schedule);
            CloudQueueMessage message = new CloudQueueMessage(scheduleJson);

            await scheduleQueue.AddMessageAsync(message);

            var result = req.CreateResponse(HttpStatusCode.Accepted);

            return result;
        }

        [FunctionName("DeleteLaunchScheduleAsyncV1")]
        public static async Task<HttpResponseMessage> DeleteLaunchSchedule(
          [HttpTrigger(AuthorizationLevel.Anonymous, "delete",
            Route = "v1/launch/schedules/async/{id}")]HttpRequestMessage req,
            [Queue("ScheduleDeleteRequests", Connection = "ScheduleStorageAccount")]CloudQueue scheduleQueue,
            string id,
            TraceWriter log)
        {

            CloudQueueMessage message = new CloudQueueMessage(id);

            await scheduleQueue.AddMessageAsync(message);

            var result = req.CreateResponse(HttpStatusCode.Accepted);

            return req.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}
