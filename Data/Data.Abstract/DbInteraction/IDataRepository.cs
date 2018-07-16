using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;

namespace Data.Abstract.DbInteraction {

//logging db operations exceptions from lower level IDbDataSource
//reporting status but not the error through IDbOperationResult
public interface IDataRopository {
    Task<IDbOperationResult<MonitoringSession>> GetNewSession(
        int profileID,
        IEnumerable<int> monitoredTagsIDList
    );
    Task<IDbOperationResult<MonitoringPulseResult>> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    );
    Task<IDbOperationVoidResult> ClearEmptySessions();

    Task<IDbOperationResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileID);
    //includes messages
    Task<IDbOperationResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionID);

    //for input validation
    Task<IDbOperationResult<bool>> HasChildren(int nodeID);
    Task<IDbOperationResult<bool>> CheckIfNodeExists(int nodeID);
    Task<IDbOperationResult<bool>> CheckIfProfileExists(int profileID);
    Task<IDbOperationResult<bool>> CheckIfTagExists(int tagID);
    Task<IDbOperationResult<bool>> CheckIfTagNameExists(string name);
    Task<IDbOperationResult<bool>> CheckIfNodeNameExists(string name);
    Task<IDbOperationResult<bool>> CheckIfCWSNameExists(string name);
    Task<IDbOperationResult<bool>> CheckIfNodeInSubtree(int nodeID, int subtreeRootNodeID);

    Task<IDbOperationVoidResult> MoveNodesSubtree(int nodeID, int newParentID);
    Task<IDbOperationResult<IEnumerable<Profile>>> GetAllProfiles();
    Task<IDbOperationResult<IEnumerable<NtwkNode>>> GetAllNodes();
    Task<IDbOperationResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagID);
    Task<IDbOperationResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID);
    Task<IDbOperationResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID);
    Task<IDbOperationResult<IEnumerable<NodeTag>>> GetAllTags();
    Task<IDbOperationResult<IEnumerable<CustomWebService>>> GetAllCVS();
    

    Task<IDbOperationResult<Profile>> CreateProfile(Profile profile);
    Task<IDbOperationResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node);
    Task<IDbOperationResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentID);
    Task<IDbOperationResult<NodeTag>> CreateTag(NodeTag tag);
    Task<IDbOperationVoidResult> CreateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);
    Task<IDbOperationResult<CustomWebService>> CreateCustomWebService(CustomWebService cws);

    Task<IDbOperationVoidResult> SetNodeTags(int nodeID, IEnumerable<int> tagIDs);
    Task<IDbOperationVoidResult> SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<IDbOperationVoidResult> SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(int profileID);
    Task<IDbOperationVoidResult> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<IDbOperationVoidResult> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID);

    Task<IDbOperationVoidResult> UpdateProfile(Profile profile);
    Task<IDbOperationVoidResult> UpdateNode(NtwkNode node);
    Task<IDbOperationVoidResult> UpdateCustomWebService(CustomWebService cws);
    Task<IDbOperationVoidResult> UpdateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);

    Task<IDbOperationResult<Profile>> RemoveProfile(int profileID);
    Task<IDbOperationResult<NtwkNode>> RemoveNode(int nodeID);
    Task<IDbOperationResult<NodeTag>> RemoveTag(int nodeID);
    Task<IDbOperationResult<CustomWebService>> RemoveCustomWebService(int cwsID);
    Task<IDbOperationVoidResult> RemoveWebServiceBinding(int nodeID, int cwsID);
}

}