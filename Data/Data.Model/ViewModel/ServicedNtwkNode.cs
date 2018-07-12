using System.Collections.Generic;

namespace Data.Model.ViewModel {

public class NtwkNode {
    public int ID;
    public int ParentID;
    public int NetworkDepth;
    public string Name;
    public string ipStr;
    public IEnumerable<int> TagIDs;
    public IEnumerable<int> WebServiceIDs;
    public bool IsOpenSSH, IsOpenTelnet, IsOpenPing;
}

}