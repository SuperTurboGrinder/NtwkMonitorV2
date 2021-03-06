// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data.Model.EFDbModel
{
    public class TagAttachment
    {
        public int ID { get; set; }
        public int TagID { get; set; }
        public NodeTag Tag { get; set; }
        public int NodeID { get; set; }
        public NtwkNode Node { get; set; }
    }
}