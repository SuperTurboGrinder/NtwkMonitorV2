using System.ComponentModel.DataAnnotations;
using Data.Model.Enums;

// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data.Model.EFDbModel
{
//for logging actual problems
    public class MonitoringMessage
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int ID { get; set; }

        [Required] public MonitoringMessageType MessageType { get; set; }

        //string in case of change or removal of the original node
        [Required] public string MessageSourceNodeName { get; set; }
        public int NumSkippedChildren { get; set; }
    }
}