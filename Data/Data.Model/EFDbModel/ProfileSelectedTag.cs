using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Data.Model.Enums;

namespace Data.Model.EFDbModel {

public class ProfileSelectedTag {
    public int ID { get; set; }
    public ProfileSelectedTagFlags Flags { get; set; }
    public int BindedProfileID { get; set; }
    public Profile BindedProfile { get; set; }
    public int TagID { get; set; }
    public NodeTag Tag { get; set; }
}

}