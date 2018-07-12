namespace Data.Model.ViewModel {

//Session can not be created from this class,
//and can not be updated by it.
//So no validation needed.
public class MonitoringSession {
    public int ID;
    public int MonitorinGroupTagID;
    public int ParticipatingNodesNum;
    public string CreationTime;
    public string LastPulseTime;
}

}