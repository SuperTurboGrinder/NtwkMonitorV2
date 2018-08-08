using System.Collections.Generic;

namespace Data.Model.ViewModel {


public class CWSBondExistanceData {
    public int nodeID;
    public bool[] bind;
}

public class CWSBondExistanceMapping {
    public int[] cwsIDs;
    public List<CWSBondExistanceData> bindings;
}

}