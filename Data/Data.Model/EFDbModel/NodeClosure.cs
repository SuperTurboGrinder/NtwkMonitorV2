
// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data.Model.EFDbModel
{
    public class NodeClosure
    {
        public int ID { get; set; }
        public int? AncestorID { get; set; }
        public NtwkNode Ancestor { get; set; }
        public int DescendantID { get; set; }
        public NtwkNode Descendant { get; set; }
        public int Distance { get; set; }
    }
}