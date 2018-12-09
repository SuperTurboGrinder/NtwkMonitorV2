namespace Data.Model.ViewModel {

public class Profile {
    public int ID;
    public string Name;
    public int MonitoringStartHour; //from 0 to 23
    public int MonitoringSessionDuration; //from 1 to 24
    public bool StartMonitoringOnLaunch;
    public bool DepthMonitoring;
    public int MonitorInterval; //minutes
    public bool RealTimePingUIUpdate;
}

}