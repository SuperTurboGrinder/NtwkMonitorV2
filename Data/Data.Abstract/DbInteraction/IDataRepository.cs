using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.EFDbModel;

namespace Data.Abstract.DbInteraction {

//logging db operations exceptions from lower level IDbDataSource
//reporting status but not the error through DbOperationResult
public interface IDataRepository {
    Task<DbOperationResult<MonitoringSession>> GetNewSession(int profileID);
    
    Task<DbOperationResult<MonitoringPulseResult>> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    );
    Task<DbOperationVoidResult> ClearEmptySessions();

    Task<DbOperationResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileID);
    //includes messages
    Task<DbOperationResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionID);

    //for input validation
    Task<DbOperationResult<bool>> HasChildren(int nodeID);
    Task<DbOperationResult<bool>> CheckIfNodeExists(int nodeID);
    Task<DbOperationResult<bool>> CheckIfProfileExists(int profileID);
    Task<DbOperationResult<bool>> CheckIfSessionExists(int sessionID);
    Task<DbOperationResult<bool>> CheckIfCWSBindingExists(
        int nodeID,
        int webServiceID
    );
    Task<DbOperationResult<bool>> CheckIfTagExists(int tagID);
    Task<DbOperationResult<bool>> CheckIfTagsExist(IEnumerable<int> tagsIDs);
    Task<DbOperationResult<bool>> CheckIfProfileNameExists(string name);
    Task<DbOperationResult<bool>> CheckIfTagNameExists(string name);
    Task<DbOperationResult<bool>> CheckIfNodeNameExists(string name);
    Task<DbOperationResult<bool>> CheckIfCWSNameExists(string name);
    Task<DbOperationResult<bool>> CheckIfNodeInSubtree(int nodeID, int subtreeRootNodeID);
    Task<DbOperationResult<int>> GetCWSParamNumber(int profileID);

    Task<DbOperationVoidResult> MoveNodesSubtree(int nodeID, int newParentID);
    Task<DbOperationResult<IEnumerable<Profile>>> GetAllProfiles();
    Task<DbOperationResult<Model.IntermediateModel.AllRawNodesData>> GetAllNodesData();
    Task<DbOperationResult<uint>> GetNodeIP(int nodeID);
    Task<DbOperationResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagID);
    Task<DbOperationResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID);
    Task<DbOperationResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID);
    Task<DbOperationResult<IEnumerable<NodeTag>>> GetAllTags();
    Task<DbOperationResult<IEnumerable<CustomWebService>>> GetAllCWS();
    Task<DbOperationResult<string>> GetCWSBoundingString(int nodeID, int cwsID);

    Task<DbOperationResult<Profile>> CreateProfile(Profile profile);
    Task<DbOperationResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node);
    Task<DbOperationResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentID);
    Task<DbOperationResult<NodeTag>> CreateTag(NodeTag tag);
    Task<DbOperationVoidResult> CreateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);
    Task<DbOperationResult<CustomWebService>> CreateCustomWebService(CustomWebService cws);

    Task<DbOperationVoidResult> SetNodeTags(int nodeID, IEnumerable<int> tagIDs);
    Task<DbOperationVoidResult> SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<DbOperationVoidResult> SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileID);
    Task<DbOperationVoidResult> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<DbOperationVoidResult> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID);

    Task<DbOperationVoidResult> UpdateProfile(Profile profile);
    Task<DbOperationVoidResult> UpdateNode(NtwkNode node);
    Task<DbOperationVoidResult> UpdateCustomWebService(CustomWebService cws);
    Task<DbOperationVoidResult> UpdateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);

    Task<DbOperationResult<Profile>> RemoveProfile(int profileID);
    Task<DbOperationResult<NtwkNode>> RemoveNode(int nodeID);
    Task<DbOperationResult<NodeTag>> RemoveTag(int tagID);
    Task<DbOperationResult<CustomWebService>> RemoveCustomWebService(int cwsID);
    Task<DbOperationVoidResult> RemoveWebServiceBinding(int nodeID, int cwsID);
}

}