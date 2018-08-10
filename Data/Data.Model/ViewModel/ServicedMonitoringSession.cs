using System;

namespace Data.Model.ViewModel {

//Session can not be created from this class,
//and can not be updated by it.
//So no validation needed.
public class MonitoringSession {
    public int ID;
    public int CreatedByProfileID;
    public int ParticipatingNodesNum;
    public double CreationTime;
    public double LastPulseTime;
}

}