using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through DataActionResult
public interface INodeTreeDataService {
    Task<DataActionResult<AllNodesData>> GetAllNodesData();
    Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagID);

    Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node);
    Task<DataActionResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentID);

    Task<StatusMessage> SetNodeTags(int nodeID, IEnumerable<int> tagIDs);
    Task<StatusMessage> MoveNodesSubtree(int nodeID, int newParentID);

    Task<StatusMessage> UpdateNode(NtwkNode node);
    Task<DataActionResult<NtwkNode>> RemoveNode(int nodeID);
}

}