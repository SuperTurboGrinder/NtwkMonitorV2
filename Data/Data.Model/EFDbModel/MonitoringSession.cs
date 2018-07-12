using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Model.EFDbModel {

//Created on program start if monitoring is enabled
//New session also made if current monitoring group was changed
//or if new pulse recived after midnight
public class MonitoringSession {
    public int ID { get; set; }
    [Required] public int CreatedByProfileID { get; set; }
    public Profile CreatedByProfile { get; set; }
    public int ParticipatingNodesNum { get; set; }
    public double CreationTime { get; set; }
    public double LastPulseTime { get; set; }
    public ICollection<MonitoringPulseResult> Pulses { get; set; }
}

}