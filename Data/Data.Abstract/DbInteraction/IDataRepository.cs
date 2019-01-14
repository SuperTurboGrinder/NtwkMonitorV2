using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Model.EFDbModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.DbInteraction
{
//logging db operations exceptions from lower level IDbDataSource
//reporting status but not the error through DbOperationResult
    public interface IDataRepository
    {
        Task<DataActionResult<MonitoringSession>> GetNewSession(int profileId);

        Task<DataActionResult<MonitoringPulseResult>> SavePulseResult(
            int sessionId,
            MonitoringPulseResult pulseResult,
            IEnumerable<MonitoringMessage> messages
        );

        Task<StatusMessage> ClearEmptySessions();

        Task<DataActionResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileId);

        //includes messages
        Task<DataActionResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionId);

        //for input validation
        Task<DataActionResult<bool>> HasChildren(int nodeId);
        Task<DataActionResult<bool>> CheckIfNodeExists(int nodeId);
        Task<DataActionResult<bool>> CheckIfProfileExists(int profileId);
        Task<DataActionResult<bool>> CheckIfSessionExists(int sessionId);

        Task<DataActionResult<bool>> CheckIfCwsBindingExists(
            int nodeId,
            int webServiceId
        );

        Task<DataActionResult<bool>> CheckIfTagExists(int tagId);
        Task<DataActionResult<bool>> CheckIfTagsExist(IEnumerable<int> tagsIDs);
        Task<DataActionResult<bool>> CheckIfProfileNameExists(string name, int? exceptId);
        Task<DataActionResult<bool>> CheckIfTagNameExists(string name, int? exceptId);
        Task<DataActionResult<bool>> CheckIfNodeNameExists(string name, int? exceptId);
        Task<DataActionResult<bool>> CheckIfCwsNameExists(string name, int? exceptId);
        Task<DataActionResult<bool>> CheckIfNodeInSubtree(int nodeId, int subtreeRootNodeId);
        Task<DataActionResult<int>> GetCwsParamNumber(int profileId);

        Task<StatusMessage> MoveNodesSubtree(int nodeId, int newParentId);
        Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles();
        Task<DataActionResult<Model.IntermediateModel.AllRawNodesData>> GetAllNodesData();
        Task<DataActionResult<uint>> GetNodeIp(int nodeId);
        Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagId);
        Task<DataActionResult<Model.ViewModel.TagFilterData>> GetProfileViewTagFilterData(int profileId);
        Task<DataActionResult<Model.ViewModel.TagFilterData>> GetProfileMonitorTagFilterData(int profileId);
        Task<DataActionResult<IEnumerable<NodeTag>>> GetAllTags();
        Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCws();
        Task<DataActionResult<string>> GetCwsBoundingString(int nodeId, int cwsId);

        Task<DataActionResult<Profile>> CreateProfile(Profile profile);
        Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node);
        Task<DataActionResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentId);
        Task<DataActionResult<NodeTag>> CreateTag(NodeTag tag);

        Task<StatusMessage> CreateWebServiceBinding(int nodeId, int cwsId,
            string param1, string param2, string param3);

        Task<DataActionResult<CustomWebService>> CreateCustomWebService(CustomWebService cws);

        Task<StatusMessage> SetNodeTags(int nodeId, IEnumerable<int> tagIDs);
        Task<StatusMessage> SetProfileViewTagsSelection(int profileId, IEnumerable<int> tagIDs);
        Task<StatusMessage> SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileId);
        Task<StatusMessage> SetProfileMonitorTagsSelection(int profileId, IEnumerable<int> tagIDs);
        Task<StatusMessage> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileId);

        Task<StatusMessage> UpdateProfile(Profile profile);
        Task<StatusMessage> UpdateTag(NodeTag tag);
        Task<StatusMessage> UpdateNode(NtwkNode node);
        Task<StatusMessage> UpdateCustomWebService(CustomWebService cws);

        Task<StatusMessage> UpdateWebServiceBinding(int nodeId, int cwsId,
            string param1, string param2, string param3);

        Task<DataActionResult<Profile>> RemoveProfile(int profileId);
        Task<DataActionResult<NtwkNode>> RemoveNode(int nodeId);
        Task<DataActionResult<NodeTag>> RemoveTag(int tagId);
        Task<DataActionResult<CustomWebService>> RemoveCustomWebService(int cwsId);
        Task<StatusMessage> RemoveWebServiceBinding(int nodeId, int cwsId);
    }
}