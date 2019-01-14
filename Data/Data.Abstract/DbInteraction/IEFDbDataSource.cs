using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Model.EFDbModel;

namespace Data.Abstract.DbInteraction
{
//only raw ef operations
//no validation
//no exception handling
    public interface IEfDbDataSource
    {
        Task<MonitoringSession> GetNewSession(
            int profileId
        );

        Task<MonitoringPulseResult> SavePulseResult(
            int sessionId,
            MonitoringPulseResult pulseResult,
            IEnumerable<MonitoringMessage> messages
        );

        Task ClearEmptySessions();

        Task<IEnumerable<MonitoringSession>> GetSessionsForProfile(int profileId);

        //includes messages
        Task<IEnumerable<MonitoringPulseResult>> GetSessionReport(int monitoringSessionId);

        //for input validation
        Task<bool> HasChildren(int nodeId);
        Task<bool> CheckIfNodeExists(int nodeId);
        Task<bool> CheckIfProfileExists(int profileId);
        Task<bool> CheckIfSessionExists(int sessionId);
        Task<bool> CheckIfCwsBindingExists(int cwsId, int nodeId);
        Task<bool> CheckIfTagExists(int tagId);
        Task<bool> CheckIfTagsExist(IEnumerable<int> tagsIDs);
        Task<bool> CheckIfTagNameExists(string name, int? exceptId);
        Task<bool> CheckIfNodeNameExists(string name, int? exceptId);
        Task<bool> CheckIfCwsNameExists(string name, int? exceptId);
        Task<bool> CheckIfProfileNameExists(string name, int? exceptId);
        Task<bool> CheckIfNodeInSubtree(int nodeId, int subtreeRootNodeId);
        Task<int> GetCwsParamNumber(int cwsId); //-1 if cws does not exist

        Task MoveNodesSubtree(int nodeId, int newParentId);
        Task<IEnumerable<Profile>> GetAllProfiles();
        Task<Model.IntermediateModel.AllRawNodesData> GetAllNodesData();
        Task<uint> GetNodeIp(int nodeId);
        Task<IEnumerable<int>> GetTaggedNodesIDs(int tagId);
        Task<Model.ViewModel.TagFilterData> GetProfileViewTagFilterData(int profileId);
        Task<Model.ViewModel.TagFilterData> GetProfileMonitorTagFilterData(int profileId);
        Task<IEnumerable<NodeTag>> GetAllTags();
        Task<IEnumerable<CustomWebService>> GetAllCws();
        Task<string> GetCwsBoundingString(int nodeId, int cwsId);

        Task<Profile> CreateProfile(Profile profile);
        Task<NtwkNode> CreateNodeOnRoot(NtwkNode node);
        Task<NtwkNode> CreateNodeWithParent(NtwkNode node, int parentId);
        Task<NodeTag> CreateTag(NodeTag tag);

        Task CreateWebServiceBinding(int nodeId, int cwsId,
            string param1, string param2, string param3);

        Task<CustomWebService> CreateCustomWebService(CustomWebService cws);

        Task SetNodeTags(int nodeId, IEnumerable<int> tagIDs);
        Task SetProfileViewTagsSelection(int profileId, IEnumerable<int> tagIDs);
        Task SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileId);
        Task SetProfileMonitorTagsSelection(int profileId, IEnumerable<int> tagIDs);
        Task SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileId);

        Task UpdateProfile(Profile profile);
        Task UpdateTag(NodeTag tag);
        Task UpdateNode(NtwkNode node);
        Task UpdateCustomWebService(CustomWebService cws);

        Task UpdateWebServiceBinding(int nodeId, int cwsId,
            string param1, string param2, string param3);

        Task<Profile> RemoveProfile(int profileId);
        Task<NtwkNode> RemoveNode(int nodeId);
        Task<NodeTag> RemoveTag(int nodeId);
        Task<CustomWebService> RemoveCustomWebService(int cwsId);
        Task RemoveWebServiceBinding(int nodeId, int cwsId);
    }
}