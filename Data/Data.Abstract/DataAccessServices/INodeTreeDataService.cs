using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through DataActionResult
public interface INodeTreeDataService {
    Task<DataActionResult<IEnumerable<NtwkNode>>> GetAllNodes();
    Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagID);

    Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node);
    Task<DataActionResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentID);

    Task<DataActionVoidResult> SetNodeTags(int nodeID, IEnumerable<int> tagIDs);
    Task<DataActionVoidResult> MoveNodesSubtree(int nodeID, int newParentID);

    Task<DataActionVoidResult> UpdateNode(NtwkNode node);
    Task<DataActionResult<NtwkNode>> RemoveNode(int nodeID);
}

}