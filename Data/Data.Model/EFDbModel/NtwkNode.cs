using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Model.EFDbModel {

public class NtwkNode {
    public int ID { get; set; }

    public int? ParentID { get; set; }
    public NtwkNode Parent { get; set; }
    public int? ParentPort { get; set; }
    public ICollection<NtwkNode> Children { get; set; }

    [Required] public string Name { get; set; }

    public ICollection<NodeClosure> Ancestors { get; set; }
    public ICollection<NodeClosure> Descendants { get; set; }
    
    public ICollection<TagAttachment> Tags { get; set; }
    public ICollection<CustomWSBinding> CustomWebServices { get; set; }

    public uint ip { get; set; }
    public bool OpenTelnet { get; set; }
    public bool OpenSSH { get; set; }
    public bool OpenPing { get; set; }
}

}