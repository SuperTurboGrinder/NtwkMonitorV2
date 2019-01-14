namespace Data.Model.ViewModel
{
//Session can not be created from this class,
//and can not be updated by it.
//So no validation needed.
    public class MonitoringSession
    {
        public int Id;
        public int CreatedByProfileId;
        public int ParticipatingNodesNum;
        public double CreationTime;
        public double LastPulseTime;
    }
}