using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.Model.EFDbModel {

public class CustomWSBinding {
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