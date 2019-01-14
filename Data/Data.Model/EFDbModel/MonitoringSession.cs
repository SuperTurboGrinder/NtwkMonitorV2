using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data.Model.EFDbModel
{
//Created on program start if monitoring is enabled
//New session also made if current monitoring group was changed
//or if new pulse received after midnight
    public class MonitoringSession
    {
        public int ID { get; set; }
        [Required] public int CreatedByProfileID { get; set; }
        public Profile CreatedByProfile { get; set; }
        public int ParticipatingNodesNum { get; set; }
        public double CreationTime { get; set; }
        public double LastPulseTime { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICollection<MonitoringPulseResult> Pulses { get; set; }
    }
}