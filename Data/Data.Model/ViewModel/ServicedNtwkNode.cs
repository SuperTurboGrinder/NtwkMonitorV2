using System.Collections.Generic;

namespace Data.Model.ViewModel {

public class NtwkNode {
    public int ID;
    public int? ParentID;
    public int? ParentPort;
    public string Name;
    public string ipStr;
    public bool IsOpenSSH, IsOpenTelnet, IsOpenPing;
}

}