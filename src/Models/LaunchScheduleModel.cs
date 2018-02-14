using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchSchedule.Functions.Models
{
    public class LaunchScheduleModel : TableEntity
    {
        //We need to add PartitionKey and RowKey for Table Entity

        public LaunchScheduleModel()
        {
            this.PartitionKey = "Schedules";
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime LaunchDate { get; set; }

        [Required]
        public string RocketType { get; set; }
    }
}
