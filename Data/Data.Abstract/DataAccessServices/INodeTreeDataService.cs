using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through IDataActionResult
public interface INetworkDataService {
    
    //for input validation
    Task<IDataActionResult<bool>> HasChildren(int nodeID);
    Task<IDataActionResult<bool>> CheckIfNodeExists(int nodeID);
    Task<IDataActionResult<bool>> CheckIfTagExists(int tagID);
    Task<IDataActionResult<bool>> CheckIfTagNameExists(string name);
    Task<IDataActionResult<bool>> CheckIfNodeNameExists(string name);
    Task<IDataActionResult<bool>> CheckIfCWSNameExists(string name);
    Task<IDataActionResult<bool>> CheckIfNodeInSubtree(int nodeID, int subtreeRootNodeID);

    Task<IDataActionVoidResult> MoveNodesSubtree(int nodeID, int newParentID);
    Task<IDataActionResult<IEnumerable<NtwkNode>>> GetAllNodes();
    Task<IDataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagID);
    Task<IDataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID);
    Task<IDataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID);
    Task<IDataActionResult<IEnumerable<NodeTag>>> GetAllTags();
    Task<IDataActionResult<IEnumerable<CustomWebService>>> GetAllCVS();
    

    Task<IDataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node);
    Task<IDataActionResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentID);
    Task<IDataActionResult<NodeTag>> CreateTag(NodeTag tag);
    Task<IDataActionVoidResult> CreateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);
    Task<IDataActionResult<CustomWebService>> CreateCustomWebService(CustomWebService cws);

    Task<IDataActionVoidResult> SetNodeTags(int nodeID, IEnumerable<int> tagIDs);

    Task<IDataActionVoidResult> UpdateNode(NtwkNode node);
    Task<IDataActionVoidResult> UpdateCustomWebService(CustomWebService cws);
    Task<IDataActionVoidResult> UpdateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);

    Task<IDataActionResult<NtwkNode>> RemoveNode(int nodeID);
    Task<IDataActionResult<NodeTag>> RemoveTag(int nodeID);
    Task<IDataActionResult<CustomWebService>> RemoveCustomWebService(int cwsID);
    Task<IDataActionVoidResult> RemoveWebServiceBinding(int nodeID, int cwsID);
}

}