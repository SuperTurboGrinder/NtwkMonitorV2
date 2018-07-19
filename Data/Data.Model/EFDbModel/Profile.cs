using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Model.EFDbModel {

public class Profile {
    public int ID { get; set; }
    [Required] public string Name { get; set; }

    public bool SendMonitoringAlarm { get; set; }
    public string MonitoringAlarmEmail { get; set; }

    public ICollection<MonitoringSession> MonitoringSessions { get; set; }
    public ICollection<ProfileSelectedTag> FilterTagSelection { get; set; }
    public int MonitoringStartHour { get; set; } //0-23
    public int MonitoringEndHour { get; set; }
    //will start monitor service with DefaultDisplayTag
    public bool StartMonitoringOnLaunch { get; set; }
    //will skip children if there is no response from the parent
    public bool DepthMonitoring { get; set; }
    public int MonitorInterval { get; set; } //in minutes
}

}