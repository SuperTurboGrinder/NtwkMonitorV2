using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through DataActionResult
public interface INodeTreeDataService {
    Task<DataActionVoidResult> MoveNodesSubtree(int nodeID, int newParentID);
    Task<DataActionResult<IEnumerable<NtwkNode>>> GetAllNodes();
    Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagID);
    Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID);
    Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID);
    Task<DataActionResult<IEnumerable<NodeTag>>> GetAllTags();
    Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCWS();
    

    Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node);
    Task<DataActionResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentID);
    Task<DataActionResult<NodeTag>> CreateTag(NodeTag tag);
    Task<DataActionVoidResult> CreateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);
    Task<DataActionResult<CustomWebService>> CreateCustomWebService(CustomWebService cws);

    Task<DataActionVoidResult> SetNodeTags(int nodeID, IEnumerable<int> tagIDs);

    Task<DataActionVoidResult> UpdateNode(NtwkNode node);
    Task<DataActionVoidResult> UpdateCustomWebService(CustomWebService cws);
    Task<DataActionVoidResult> UpdateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);

    Task<DataActionResult<NtwkNode>> RemoveNode(int nodeID);
    Task<DataActionResult<NodeTag>> RemoveTag(int tagID);
    Task<DataActionResult<CustomWebService>> RemoveCustomWebService(int cwsID);
    Task<DataActionVoidResult> RemoveWebServiceBinding(int nodeID, int cwsID);
}

}