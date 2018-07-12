using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Model.EFDbModel {

//Created for every monitor iteration
//(After all tagged nodes are returned ping or was skipped)
public class MonitoringPulseResult {
    public int ID { get; set; }
    public int Responded { get; set; }
    public int Silent { get; set; }
    public int Skipped { get; set; }
    public double CreationTime { get; set; }
    public ICollection<MonitoringMessage> Messages { get; set; }
}

}