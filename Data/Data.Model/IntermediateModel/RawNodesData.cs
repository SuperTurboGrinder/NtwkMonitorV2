using System.Collections.Generic;

using EFDbModel = Data.Model.EFDbModel; 

namespace Data.Model.IntermediateModel {

public class RawNodeData {
    public EFDbModel.NtwkNode Node;
    public int[] TagsIDs;
    public int[] BoundWebServicesIDs;
}

}