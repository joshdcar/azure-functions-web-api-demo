using System;
using System.Threading.Tasks;
using LaunchSchedule.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace LaunchSchedule.Functions
{

    public static class LaunchScheduleQueues
    {
        [FunctionName("LaunchScheduleCreateQueue")]
        public static async Task LaunchScheduleCreateQueue(
            [QueueTrigger("ScheduleCreateRequests", Connection = "ScheduleStorageAccount")]string scheduleJson,
            [Table("LaunchSchedules", Connection = "ScheduleStorageAccount")]CloudTable scheduleTable,
            TraceWriter log)
        {

            log.Info($"Schedule Create Request Item Processed: {scheduleJson}");

            var schedule = JsonConvert.DeserializeObject<LaunchScheduleModel>(scheduleJson);

            TableOperation easyInsertOperation = TableOperation.Insert(schedule);
            await scheduleTable.ExecuteAsync(easyInsertOperation);

            log.Info($"Schedule Successfully Created.");
        }


        [FunctionName("LaunchScheduleUpdateQueue")]
        public static async Task LaunchScheduleUpdateQueue(
            [QueueTrigger("ScheduleUpdateRequests", Connection = "ScheduleStorageAccount")]string scheduleJson,
            [Table("LaunchSchedules", Connection = "ScheduleStorageAccount")]CloudTable scheduleTable,
            TraceWriter log)
        {

            log.Info($"Schedule Update Request Item Processed: {scheduleJson}");

            var schedule = JsonConvert.DeserializeObject<LaunchScheduleModel>(scheduleJson);

            TableOperation updateOperation = TableOperation.Replace(schedule);
            await scheduleTable.ExecuteAsync(updateOperation);

            log.Info($"Schedule Successfully Updated.");
        }

        [FunctionName("LaunchScheduleDeleteQueue")]
        public static async Task LaunchScheduleDeleteQueue(
            [QueueTrigger("ScheduleDeleteRequests", Connection = "ScheduleStorageAccount")]string id,
            [Table("LaunchSchedules", Connection = "ScheduleStorageAccount")]CloudTable scheduleTable,
            TraceWriter log)
        {

            log.Info($"Schedule Delete Request Item Processed: {id}");

            TableEntity entity = new TableEntity("Schedules", id);

            TableOperation deleteOperation = TableOperation.Delete(entity);
            await scheduleTable.ExecuteAsync(deleteOperation);

            log.Info($"Schedule Successfully Deleted.");
        }
    }
}
