using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.EFDbModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.DbInteraction {

//logging db operations exceptions from lower level IDbDataSource
//reporting status but not the error through DbOperationResult
public interface IDataRepository {
    Task<DataActionResult<MonitoringSession>> GetNewSession(int profileID);
    
    Task<DataActionResult<MonitoringPulseResult>> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    );
    Task<StatusMessage> ClearEmptySessions();

    Task<DataActionResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileID);
    //includes messages
    Task<DataActionResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionID);

    //for input validation
    Task<DataActionResult<bool>> HasChildren(int nodeID);
    Task<DataActionResult<bool>> CheckIfNodeExists(int nodeID);
    Task<DataActionResult<bool>> CheckIfProfileExists(int profileID);
    Task<DataActionResult<bool>> CheckIfSessionExists(int sessionID);
    Task<DataActionResult<bool>> CheckIfCWSBindingExists(
        int nodeID,
        int webServiceID
    );
    Task<DataActionResult<bool>> CheckIfTagExists(int tagID);
    Task<DataActionResult<bool>> CheckIfTagsExist(IEnumerable<int> tagsIDs);
    Task<DataActionResult<bool>> CheckIfProfileNameExists(string name, int? exceptID);
    Task<DataActionResult<bool>> CheckIfTagNameExists(string name, int? exceptID);
    Task<DataActionResult<bool>> CheckIfNodeNameExists(string name, int? exceptID);
    Task<DataActionResult<bool>> CheckIfCWSNameExists(string name, int? exceptID);
    Task<DataActionResult<bool>> CheckIfNodeInSubtree(int nodeID, int subtreeRootNodeID);
    Task<DataActionResult<int>> GetCWSParamNumber(int profileID);

    Task<StatusMessage> MoveNodesSubtree(int nodeID, int newParentID);
    Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles();
    Task<DataActionResult<Model.IntermediateModel.AllRawNodesData>> GetAllNodesData();
    Task<DataActionResult<uint>> GetNodeIP(int nodeID);
    Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagID);
    Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID);
    Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID);
    Task<DataActionResult<IEnumerable<NodeTag>>> GetAllTags();
    Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCWS();
    Task<DataActionResult<string>> GetCWSBoundingString(int nodeID, int cwsID);

    Task<DataActionResult<Profile>> CreateProfile(Profile profile);
    Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node);
    Task<DataActionResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentID);
    Task<DataActionResult<NodeTag>> CreateTag(NodeTag tag);
    Task<StatusMessage> CreateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);
    Task<DataActionResult<CustomWebService>> CreateCustomWebService(CustomWebService cws);

    Task<StatusMessage> SetNodeTags(int nodeID, IEnumerable<int> tagIDs);
    Task<StatusMessage> SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<StatusMessage> SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileID);
    Task<StatusMessage> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<StatusMessage> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID);

    Task<StatusMessage> UpdateProfile(Profile profile);
    Task<StatusMessage> UpdateTag(NodeTag tag);
    Task<StatusMessage> UpdateNode(NtwkNode node);
    Task<StatusMessage> UpdateCustomWebService(CustomWebService cws);
    Task<StatusMessage> UpdateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);

    Task<DataActionResult<Profile>> RemoveProfile(int profileID);
    Task<DataActionResult<NtwkNode>> RemoveNode(int nodeID);
    Task<DataActionResult<NodeTag>> RemoveTag(int tagID);
    Task<DataActionResult<CustomWebService>> RemoveCustomWebService(int cwsID);
    Task<StatusMessage> RemoveWebServiceBinding(int nodeID, int cwsID);
}

}