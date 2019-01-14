using Data.Model.Enums;

// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data.Model.EFDbModel
{
    public class ProfileSelectedTag
    {
        public int ID { get; set; }
        public ProfileSelectedTagFlags Flags { get; set; }
        public int BindedProfileID { get; set; }
        public Profile BindedProfile { get; set; }
        public int TagID { get; set; }
        public NodeTag Tag { get; set; }
    }
}