using System.Collections.Generic;
using System.Threading.Tasks;

using Data.Model.EFDbModel;

namespace Data.Abstract.DbInteraction {

//only raw ef operations
//no validation
//no exception handling
public interface IEFDbDataSource {
    Task<MonitoringSession> GetNewSession(
        int profileID
    );
    Task<MonitoringPulseResult> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    );
    Task ClearEmptySessions();

    Task<IEnumerable<MonitoringSession>> GetSessionsForProfile(int profileID);
    //includes messages
    Task<IEnumerable<MonitoringPulseResult>> GetSessionReport(int monitoringSessionID);

    //for input validation
    Task<bool> HasChildren(int nodeID);
    Task<bool> CheckIfNodeExists(int nodeID);
    Task<bool> CheckIfProfileExists(int profileID);
    Task<bool> CheckIfSessionExists(int sessionID);
    Task<bool> CheckIfTagExists(int tagID);
    Task<bool> CheckIfTagsExist(IEnumerable<int> tagsIDs);
    Task<bool> CheckIfTagNameExists(string name);
    Task<bool> CheckIfNodeNameExists(string name);
    Task<bool> CheckIfCWSNameExists(string name);
    Task<bool> CheckIfNodeInSubtree(int nodeID, int subtreeRootNodeID);

    Task MoveNodesSubtree(int nodeID, int newParentID);
    Task<IEnumerable<Profile>> GetAllProfiles();
    Task<IEnumerable<NtwkNode>> GetAllNodes();
    Task<IEnumerable<int>> GetTaggedNodesIDs(int tagID);
    Task<IEnumerable<int>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID);
    Task<IEnumerable<int>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID);
    Task<IEnumerable<NodeTag>> GetAllTags();
    Task<IEnumerable<CustomWebService>> GetAllCVS();
    

    Task<Profile> CreateProfile(Profile profile);
    Task<NtwkNode> CreateNodeOnRoot(NtwkNode node);
    Task<NtwkNode> CreateNodeWithParent(NtwkNode node, int parentID);
    Task<NodeTag> CreateTag(NodeTag tag);
    Task CreateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);
    Task<CustomWebService> CreateCustomWebService(CustomWebService cws);

    Task SetNodeTags(int nodeID, IEnumerable<int> tagIDs);
    Task SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileID);
    Task SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID);

    Task UpdateProfile(Profile profile);
    Task UpdateNode(NtwkNode node);
    Task UpdateCustomWebService(CustomWebService cws);
    Task UpdateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);

    Task<Profile> RemoveProfile(int profileID);
    Task<NtwkNode> RemoveNode(int nodeID);
    Task<NodeTag> RemoveTag(int nodeID);
    Task<CustomWebService> RemoveCustomWebService(int cwsID);
    Task RemoveWebServiceBinding(int nodeID, int cwsID);
}

}
