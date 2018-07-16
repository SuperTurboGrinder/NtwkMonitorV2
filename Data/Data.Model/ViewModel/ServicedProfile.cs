namespace Data.Model.ViewModel {

public class Profile {
    public int ID;
    public string Name;
    public int MonitoringStartHour; //from 0 to 24
    public int MonitoringEndHour;
    public bool StartMonitoringOnLaunch;
    public bool DepthMonitoring;
    public int MonitorInterval; //minutes
}

}