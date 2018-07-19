namespace Data.Model.ViewModel {

public class Profile {
    public int ID;
    public string Name;
    public bool SendMonitoringAlarm;
    public string MonitoringAlarmEmail;
    public int MonitoringStartHour; //from 0 to 23
    public int MonitoringEndHour;
    public bool StartMonitoringOnLaunch;
    public bool DepthMonitoring;
    public int MonitorInterval; //minutes
}

}