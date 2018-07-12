namespace Data.Model.ViewModel {

public class Network {
    public int ID;
    public string Name;
    public int DefaultDisplayGroupTagID;
    public bool MonitoringEnabled;
    //when true will skip children if there is no response from the parent
    public bool DepthMonitoring;
    public int MonitoredGroupTagID;
    public int MonitorInterval; //minutes
}

}