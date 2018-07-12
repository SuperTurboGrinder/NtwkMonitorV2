using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Model.EFDbModel {

public class NodeTag {
    public int ID { get; set; }
    [Required] public string Name { get; set; }
    public ICollection<TagAttachment> Attachments { get; set; }
    public ICollection<ProfileSelectedTag> ProfilesFilterSelections { get; set; }
}

}