using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LaunchSchedule.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace LaunchSchedule.Functions
{
    public static class LaunchSchedule
    {
        [FunctionName("GetLaunchSchedulesV1")]
        public static async Task<HttpResponseMessage> GetLaunchSchedules(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", 
                        Route = "v1/launch/schedules")]HttpRequestMessage req,
            [Table("LaunchSchedules", "Schedules")] IQueryable<LaunchScheduleModel> schedules,
            TraceWriter log)
        {
            return req.CreateResponse(HttpStatusCode.OK, schedules);
        }


        [FunctionName("GetLaunchScheduleV1")]
        public static async Task<HttpResponseMessage> GetLaunchSchedule(
            [HttpTrigger(AuthorizationLevel.Anonymous, 
                        "get", 
                         Route = "v1/launch/schedules/{id}")]HttpRequestMessage req,
            [Table("LaunchSchedules", "Schedules", "{id}", Connection = "ScheduleStorageAccount")] LaunchScheduleModel launchSchedule,
            TraceWriter log,
            string id
            )
        {

            if (launchSchedule  == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
            else
            {
                log.Info("Binding Values. Name: " + launchSchedule.Name);
            }

            return req.CreateResponse(HttpStatusCode.OK, launchSchedule);

        }

        [FunctionName("CreateLaunchScheduleV1")]
        public static async Task<HttpResponseMessage> CreateLaunchSchedule(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", 
                            Route = "v1/launch/schedules")]LaunchScheduleModel schedule, 
            HttpRequestMessage req,
            [Table("LaunchSchedules", Connection = "ScheduleStorageAccount")]CloudTable scheduleTable,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // *******************************************************
            // We can validate after the fact if we load the JSON
            // contents from the body. This allows us to get the validation
            // results and give back a better exception then just 
            // 500 Bad Request. Replace the schedule object below
            // with a version pulled from the body (JSONConvert)
            // *******************************************************

            ValidationContext validationContext = new ValidationContext(schedule);
            ICollection<ValidationResult> results = new List<ValidationResult>();
            var valid = Validator.TryValidateObject(schedule, validationContext, results, true);

            if (!valid)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid Data");
            }

            schedule.RowKey = Guid.NewGuid().ToString();

            TableOperation easyInsertOperation = TableOperation.Insert(schedule);
            await scheduleTable.ExecuteAsync(easyInsertOperation);

            var locationUrl = $"{req.RequestUri.GetLeftPart(UriPartial.Authority)}/api/launch/schedules/{schedule.RowKey}";

            var result = req.CreateResponse(HttpStatusCode.Created);
            result.Headers.Add("location", locationUrl);

            return result;

        }


        [FunctionName("UpdateLaunchScheduleV1")]
        public static async Task<HttpResponseMessage> UpdateLaunchSchedule(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put", 
                        Route = "v1/launch/schedules")]LaunchScheduleModel schedule,
           [Table("LaunchSchedules", Connection = "ScheduleStorageAccount")]CloudTable scheduleTable,
           HttpRequestMessage req, 
           TraceWriter log)
        {

            TableOperation updateOperation = TableOperation.Replace(schedule);
            await scheduleTable.ExecuteAsync(updateOperation);

            var result = req.CreateResponse(HttpStatusCode.OK);

            return result;
        }

        [FunctionName("DeleteLaunchScheduleV1")]
        public static async Task<HttpResponseMessage> DeleteLaunchSchedule(
          [HttpTrigger(AuthorizationLevel.Anonymous, "delete", 
            Route = "v1/launch/schedules/{id}")]HttpRequestMessage req,
            [Table("LaunchSchedules", Connection = "ScheduleStorageAccount")]CloudTable scheduleTable,
            string id,
            TraceWriter log)
        {

            TableEntity entity = new TableEntity("Schedules", id);

            TableOperation deleteOperation = TableOperation.Delete(entity);
            await scheduleTable.ExecuteAsync(deleteOperation);

            return req.CreateResponse(HttpStatusCode.NoContent);
        }

    }


}
