// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data.Model.EFDbModel
{
    public class CustomWsBinding
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int ID { get; set; }
        public int ServiceID { get; set; }
        public CustomWebService Service { get; set; }
        public int NodeID { get; set; }
        public NtwkNode Node { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public string Param3 { get; set; }
    }
}