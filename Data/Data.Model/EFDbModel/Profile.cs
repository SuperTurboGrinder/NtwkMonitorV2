using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data.Model.EFDbModel
{
    public class Profile
    {
        public int ID { get; set; }
        [Required] public string Name { get; set; }

        public ICollection<MonitoringSession> MonitoringSessions { get; set; }

        public ICollection<ProfileSelectedTag> FilterTagSelection { get; set; }

        //will start monitor service with DefaultDisplayTag
        public bool StartMonitoringOnLaunch { get; set; }

        //will skip children if there is no response from the parent
        public bool DepthMonitoring { get; set; }
        public int MonitorInterval { get; set; } //in minutes
        public bool RealTimePingUIUpdate { get; set; }

        //Deprecated (sqlite does not support DropColumnOperation)
        //will maybe rename them and repurpose for something else
        public int MonitoringStartHour { get; set; } //0-23
        public int MonitoringSessionDuration { get; set; } //1-24
    }
}