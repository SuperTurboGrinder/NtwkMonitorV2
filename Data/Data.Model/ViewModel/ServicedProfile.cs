namespace Data.Model.ViewModel
{
    public class Profile
    {
        public int Id;
        public string Name;
        public bool StartMonitoringOnLaunch;
        public bool DepthMonitoring;
        public int MonitorInterval; //minutes
        public bool RealTimePingUiUpdate;
    }
}