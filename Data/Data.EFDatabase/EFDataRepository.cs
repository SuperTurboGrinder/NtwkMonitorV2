using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Data.Abstract.DbInteraction;
using Data.Model.EFDbModel;

namespace Data.EFDatabase {

public class EFDataRepository : IDataRepository {
    IEFDbDataSource efDataSource;
    IDbErrorLogger logger;

    public EFDataRepository(IEFDbDataSource dataSource, IDbErrorLogger _logger) {
        efDataSource = dataSource;
        logger = _logger;
    }

    async Task<DbOperationResult<T>> PerformDbOperationAndLogDbUpdateException<T>(
        Func<Task<T>> func
    ) {
        T result = default(T);
        bool success = false;
        try{
           result = await func();
           success = true;
        }
        catch(AggregateException aggrex) {
            aggrex.Handle(ex => {
                logger.LogException(ex);
                if(ex is DbUpdateException) {
                    return true;
                }
                return false;
            });
        }
        return new DbOperationResult<T>(result, success);
    }

    async Task<DbOperationVoidResult> PerformDbOperationAndLogDbUpdateException(
        Func<Task> func
    ) {
        bool success = false;
        try{
           await func();
           success = true;
        }
        catch(AggregateException aggrex) {
            aggrex.Handle(ex => {
                logger.LogException(ex);
                if(ex is DbUpdateException) {
                    return true;
                }
                return false;
            });
        }
        return new DbOperationVoidResult(success);
    }

    public async Task<DbOperationResult<MonitoringSession>> GetNewSession(
        int profileID
    ) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetNewSession(profileID)
        );
    }

    public async Task<DbOperationResult<MonitoringPulseResult>> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    ) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.SavePulseResult(sessionID, pulseResult, messages)
        );
    }

    public async Task<DbOperationVoidResult> ClearEmptySessions() {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.ClearEmptySessions()
        );
    }
    

    public async Task<DbOperationResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetSessionsForProfile(profileID)
        );
    }
    
    //includes messages
    public async Task<DbOperationResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetSessionReport(monitoringSessionID)
        );
    }

    //for input validation
    public async Task<DbOperationResult<bool>> HasChildren(int nodeID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.HasChildren(nodeID)
        );
    }
    
    public async Task<DbOperationResult<bool>> CheckIfNodeExists(int nodeID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CheckIfNodeExists(nodeID)
        );
    }
    
    public async Task<DbOperationResult<bool>> CheckIfProfileExists(int profileID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CheckIfProfileExists(profileID)
        );
    }
    
    public async Task<DbOperationResult<int>> GetCWSParamNumber(int cwsID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetCWSParamNumber(cwsID)
        );
    }

    public async Task<DbOperationResult<bool>> CheckIfSessionExists(int sessionID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CheckIfSessionExists(sessionID)
        );
    }
    
    public async Task<DbOperationResult<bool>> CheckIfTagExists(int tagID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CheckIfTagExists(tagID)
        );
    }

    public async Task<DbOperationResult<bool>> CheckIfTagsExist(IEnumerable<int> tagsIDs) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CheckIfTagsExist(tagsIDs)
        );
    }
    
    public async Task<DbOperationResult<bool>> CheckIfTagNameExists(string name) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CheckIfTagNameExists(name)
        );
    }
    
    public async Task<DbOperationResult<bool>> CheckIfNodeNameExists(string name) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CheckIfNodeNameExists(name)
        );
    }
    
    public async Task<DbOperationResult<bool>> CheckIfProfileNameExists(string name) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CheckIfProfileNameExists(name)
        );
    }
    
    public async Task<DbOperationResult<bool>> CheckIfCWSNameExists(string name) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CheckIfCWSNameExists(name)
        );
    }
    
    public async Task<DbOperationResult<bool>> CheckIfNodeInSubtree(int nodeID, int subtreeRootNodeID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CheckIfNodeInSubtree(nodeID, subtreeRootNodeID)
        );
    }
    

    public async Task<DbOperationVoidResult> MoveNodesSubtree(int nodeID, int newParentID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.MoveNodesSubtree(nodeID, newParentID)
        );
    }
    
    public async Task<DbOperationResult<IEnumerable<Profile>>> GetAllProfiles() {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetAllProfiles()
        );
    }
    
    public async Task<DbOperationResult<Model.IntermediateModel.AllRawNodesData>> GetAllNodesData() {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetAllNodesData()
        );
    }

    public async Task<DbOperationResult<uint>> GetNodeIP(int nodeID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetNodeIP(nodeID)
        );
    }
    
    public async Task<DbOperationResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetTaggedNodesIDs(tagID)
        );
    }
    
    public async Task<DbOperationResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetIDsOfNodesBySelectedTagsInProfileView(profileID)
        );
    }
    
    public async Task<DbOperationResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetIDsOfNodesBySelectedTagsInProfileMonitor(profileID)
        );
    }
    
    public async Task<DbOperationResult<IEnumerable<NodeTag>>> GetAllTags() {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetAllTags()
        );
    }
    
    public async Task<DbOperationResult<IEnumerable<CustomWebService>>> GetAllCWS() {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetAllCWS()
        );
    }

    public async Task<DbOperationResult<Data.Model.ViewModel.CWSBondExistanceMapping>> GetCWSBondExistanceMapping() {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetCWSBondExistanceMapping()
        );
    }

    public async Task<DbOperationResult<string>> GetCWSBoundingString(
        int nodeID,
        int cwsID
    ) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.GetCWSBoundingString(nodeID, cwsID)
        );
    }
    
    

    public async Task<DbOperationResult<Profile>> CreateProfile(Profile profile) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CreateProfile(profile)
        );
    }
    
    public async Task<DbOperationResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CreateNodeOnRoot(node)
        );
    }
    
    public async Task<DbOperationResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CreateNodeWithParent(node, parentID)
        );
    }
    
    public async Task<DbOperationResult<NodeTag>> CreateTag(NodeTag tag) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CreateTag(tag)
        );
    }
    
    public async Task<DbOperationVoidResult> CreateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3
    ) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CreateWebServiceBinding(nodeID, cwsID, param1, param2, param3)
        );
    }
    
    public async Task<DbOperationResult<CustomWebService>> CreateCustomWebService(CustomWebService cws) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.CreateCustomWebService(cws)
        );
    }

    public async Task<DbOperationVoidResult> SetNodeTags(int nodeID, IEnumerable<int> tagIDs) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.SetNodeTags(nodeID, tagIDs)
        );
    }
    
    public async Task<DbOperationVoidResult> SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.SetProfileViewTagsSelection(profileID, tagIDs)
        );
    }
    
    public async Task<DbOperationVoidResult> SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.SetProfileViewTagsSelectionToProfileMonitorTagsSelection(profileID)
        );
    }
    
    public async Task<DbOperationVoidResult> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.SetProfileMonitorTagsSelection(profileID, tagIDs)
        );
    }
    
    public async Task<DbOperationVoidResult> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.SetProfileMonitorTagsSelectionToProfileViewTagsSelection(profileID)
        );
    }
    

    public async Task<DbOperationVoidResult> UpdateProfile(Profile profile) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.UpdateProfile(profile)
        );
    }
    
    public async Task<DbOperationVoidResult> UpdateNode(NtwkNode node) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.UpdateNode(node)
        );
    }
    
    public async Task<DbOperationVoidResult> UpdateCustomWebService(CustomWebService cws) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.UpdateCustomWebService(cws)
        );
    }
    
    public async Task<DbOperationVoidResult> UpdateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.UpdateWebServiceBinding(nodeID, cwsID, param1, param2, param3)
        );
    }
    

    public async Task<DbOperationResult<Profile>> RemoveProfile(int profileID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.RemoveProfile(profileID)
        );
    }
    
    public async Task<DbOperationResult<NtwkNode>> RemoveNode(int nodeID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.RemoveNode(nodeID)
        );
    }
    
    public async Task<DbOperationResult<NodeTag>> RemoveTag(int tagID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.RemoveTag(tagID)
        );
    }
    
    public async Task<DbOperationResult<CustomWebService>> RemoveCustomWebService(int cwsID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.RemoveCustomWebService(cwsID)
        );
    }
    
    public async Task<DbOperationVoidResult> RemoveWebServiceBinding(int nodeID, int cwsID) {
        return await PerformDbOperationAndLogDbUpdateException(async () =>
            await efDataSource.RemoveWebServiceBinding(nodeID, cwsID)
        );
    }
}

}