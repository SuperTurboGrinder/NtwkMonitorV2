using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Data.Abstract.DbInteraction;
using Data.Model.EFDbModel;
using static Data.EFDatabase.Logic.EFDataSourceLogic;

namespace Data.EFDatabase {

class EFDataSource : IEFDbDataSource {
    readonly NtwkDBContext context;
    readonly IDbErrorLogger errorLogger;

    EFDataSource(NtwkDBContext _context, IDbErrorLogger _errorLogger) {
        context = _context;
        errorLogger = _errorLogger;
    }

    public async Task<MonitoringSession> GetNewSession(
        int profileID
    ) {
        return await GetNewSession_Logic(context, profileID);
    }

    public async Task<MonitoringPulseResult> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    ) {
        return await SavePulseResult_Logic(context, sessionID, pulseResult, messages);
    }

    public async Task ClearEmptySessions() {
        await ClearEmptySessions_Logic(context);
    }

    public async Task<IEnumerable<MonitoringSession>> GetSessionsForProfile(int profileID) {
        return await GetSessionsForProfile_Logic(context, profileID);
    }
    
    public async Task<IEnumerable<MonitoringPulseResult>> GetSessionReport(int monitoringSessionID) {
        return await GetSessionReport_Logic(context, monitoringSessionID);
    }


    public async Task<bool> HasChildren(int nodeID) {
        return await HasChildren_Logic(context, nodeID);
    }

    public async Task<bool> CheckIfProfileExists(int profileID) {
        return await CheckIfProfileExists_Logic(context, profileID);
    }

    public async Task<int> GetCWSParamNumber(int cwsID) {
        return await GetCWSParamNumber_Logic(context, cwsID);
    }

    public async Task<bool> CheckIfSessionExists(int sessionID) {
        return await CheckIfSessionExists_Logic(context, sessionID);
    }

    public async Task<bool> CheckIfCWSBindingExists(int nodeID, int cwsID) {
        return await CheckIfCWSBindingExists_Logic(context, nodeID, cwsID);
    }

    public async Task<bool> CheckIfTagExists(int tagID) {
        return await CheckIfTagExists_Logic(context, tagID);
    }

    public async Task<bool> CheckIfTagsExist(IEnumerable<int> tagsIDs) {
        return await CheckIfTagsExist_Logic(context, tagsIDs);
    }
    
    public async Task<bool> CheckIfNodeExists(int nodeID) {
        return await CheckIfNodeExists_Logic(context, nodeID);
    }

    public async Task<bool> CheckIfTagNameExists(string name) {
        return await CheckIfTagNameExists_Logic(context, name);
    }

    public async Task<bool> CheckIfNodeNameExists(string name) {
        return await CheckIfNodeNameExists_Logic(context, name);
    }

    public async Task<bool> CheckIfCWSNameExists(string name) {
        return await CheckIfCWSNameExists_Logic(context, name);
    }

    public async Task<bool> CheckIfProfileNameExists(string name) {
        return await CheckIfProfileNameExists_Logic(context, name);
    }

    public async Task<bool> CheckIfNodeInSubtree(
        int nodeID,
        int subtreeRootNodeID
    ) {
        return await CheckIfNodeInSubtree_Logic(context, nodeID, subtreeRootNodeID);
    }
    

    public async Task MoveNodesSubtree(int nodeID, int newParentID) {
        await MoveNodesSubtree_Logic(context, nodeID, newParentID);
    }

    public async Task<IEnumerable<Profile>> GetAllProfiles() {
        return await GetAllProfiles_Logic(context);
    }
    
    public async Task<Model.IntermediateModel.AllRawNodesData> GetAllNodesData() {
        return await GetAllNodesData_Logic(context);
    }

    public async Task<uint> GetNodeIP(int nodeID) {
        return await GetNodeIP_Logic(context, nodeID);
    }
    
    public async Task<IEnumerable<int>> GetTaggedNodesIDs(int tagID) {
        return await GetTaggedNodesIDs_Logic(context, tagID);
    }

    public async Task<IEnumerable<int>> GetIDsOfNodesBySelectedTagsInProfileView(
        int profileID
    ) {
        return await GetIDsOfNodesBySelectedTagsInProfileView_Logic(context, profileID);
    }

    public async Task<IEnumerable<int>> GetIDsOfNodesBySelectedTagsInProfileMonitor(
        int profileID
    ) {
        return await GetIDsOfNodesBySelectedTagsInProfileMonitor_Logic(context, profileID);
    }
    
    public async Task<IEnumerable<NodeTag>> GetAllTags() {
        return await GetAllTags_Logic(context);
    }
    
    public async Task<IEnumerable<CustomWebService>> GetAllCWS() {
        return await GetAllCWS_Logic(context);
    }
    
    public async Task<Data.Model.ViewModel.CWSBondExistanceMapping> GetCWSBondExistanceMapping() {
        return await GetCWSBondExistanceMapping_Logic(context);
    }

    public  async Task<string> GetCWSBoundingString(int nodeID, int cwsID) {
        return await GetCWSBoundingString_Logic(context, nodeID, cwsID);
    }


    public async Task<Profile> CreateProfile(Profile profile) {
        return await CreateProfile_Logic(context, profile);
    }
    
    public async Task<NtwkNode> CreateNodeOnRoot(NtwkNode node) {
        return await CreateNodeOnRoot_Logic(context, node);
    }

    public async Task<NtwkNode> CreateNodeWithParent(NtwkNode node, int parentID) {
        return await CreateNodeWithParent_Logic(context, node, parentID);
    }
    
    public async Task<NodeTag> CreateTag(NodeTag tag) {
        return await CreateTag_Logic(context, tag);
    }

    public async Task CreateWebServiceBinding(
        int nodeID,
        int cwsID,
        string param1,
        string param2,
        string param3
    ) {
        await CreateWebServiceBinding_Logic(context, nodeID, cwsID,
            param1, param2, param3);
    }

    public async Task SetNodeTags(int nodeID, IEnumerable<int> tagIDs) {
        await SetNodeTags_Logic(context, nodeID, tagIDs);
    }

    public async Task SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        await SetProfileViewTagsSelection_Logic(context, profileID, tagIDs);
    }

    
    public async Task SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileID) {
        await SetProfileViewTagsSelectionToProfileMonitorTagsSelection_Logic(
            context, profileID);
    }

    public async Task SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        await SetProfileMonitorTagsSelection_Logic(context, profileID, tagIDs);
    }

    public async Task SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID) {
        await SetProfileMonitorTagsSelectionToProfileViewTagsSelection_Logic(
            context, profileID);
    }
    
    public async Task<CustomWebService> CreateCustomWebService(CustomWebService cws) {
        return await CreateCustomWebService_Logic(context, cws);
    }
    
    public async Task UpdateProfile(Profile profile) {
        await UpdateProfile_Logic(context, profile);
    }
    
    public async Task UpdateNode(NtwkNode node) {
        await UpdateNode_Logic(context, node);
    }
    
    public async Task UpdateCustomWebService(CustomWebService cws) {
        await UpdateCustomWebService_Logic(context, cws);
    }

    public async Task UpdateWebServiceBinding(
        int nodeID,
        int cwsID,
        string param1,
        string param2,
        string param3
    ) {
        await UpdateWebServiceBinding_Logic(context, nodeID, cwsID,
            param1, param2, param3);
    }
    

    public async Task<Profile> RemoveProfile(int profileID) {
        return await RemoveProfile_Logic(context, profileID);
    }
    
    public async Task<NtwkNode> RemoveNode(int nodeID) {
        return await RemoveNode_Logic(context, nodeID);
    }
    
    public async Task<NodeTag> RemoveTag(int nodeID) {
        return await RemoveTag_Logic(context, nodeID);
    }
    
    public async Task<CustomWebService> RemoveCustomWebService(int cwsID) {
        return await RemoveCustomWebService_Logic(context, cwsID);
    }

    public async Task RemoveWebServiceBinding(
        int nodeID,
        int cwsID
    ) {
        await RemoveWebServiceBinding_Logic(context, nodeID, cwsID);
    }
}

}