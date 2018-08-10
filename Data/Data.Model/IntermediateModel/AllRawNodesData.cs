using System.Collections.Generic;

using ViewModel = Data.Model.ViewModel;

namespace Data.Model.IntermediateModel {

public class AllRawNodesData {
    public List<ViewModel.CWSData> WebServicesData;
    public List<IEnumerable<RawNodeData>> NodesData;
}

}