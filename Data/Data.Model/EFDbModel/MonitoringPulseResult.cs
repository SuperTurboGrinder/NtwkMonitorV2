using System.Collections.Generic;

// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data.Model.EFDbModel
{
//Created for every monitor iteration
//(After all tagged nodes are returned ping or was skipped)
    public class MonitoringPulseResult
    {
        public int ID { get; set; }
        public int Responded { get; set; }
        public int Silent { get; set; }
        public int Skipped { get; set; }
        public double CreationTime { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICollection<MonitoringMessage> Messages { get; set; }
    }
}