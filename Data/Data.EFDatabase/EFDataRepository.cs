using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Data.Abstract.DbInteraction;
using Data.Model.EFDbModel;
using Data.Model.ResultsModel;

namespace Data.EFDatabase {

public class EFDataRepository : IDataRepository {
    readonly IEFDbDataSource efDataSource;
    readonly IDbErrorLogger logger;

    public EFDataRepository(IEFDbDataSource dataSource, IDbErrorLogger _logger) {
        efDataSource = dataSource;
        logger = _logger;
    }

    async Task<DataActionResult<T>> PerformDataOperationOrLogExceptions<T>(
        Func<Task<T>> getter
    ) {
        try{
           return DataActionResult<T>.Successful(await getter());
        }
        catch(AggregateException aggrex) {
            aggrex.Handle(ex => {
                logger.LogException(ex);
                return true;
            });
            return DataActionResult<T>.Failed(StatusMessage.DatabaseInternalError);
        }
        catch(Exception ex) {
            logger.LogException(ex);
            return DataActionResult<T>.Failed(StatusMessage.DatabaseInternalError);
        }
    }

    async Task<StatusMessage> PerformOperationOrLogExceptions(
        Func<Task> action
    ) {
        try{
           await action();
           return StatusMessage.Ok;
        }
        catch(AggregateException aggrex) {
            aggrex.Handle(ex => {
                logger.LogException(ex);
                return true;
            });
            return StatusMessage.DatabaseInternalError;
        }
        catch(Exception ex) {
            logger.LogException(ex);
            return StatusMessage.DatabaseInternalError;
        }
    }

    public async Task<DataActionResult<MonitoringSession>> GetNewSession(
        int profileID
    ) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetNewSession(profileID)
        );
    }

    public async Task<DataActionResult<MonitoringPulseResult>> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    ) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.SavePulseResult(sessionID, pulseResult, messages)
        );
    }

    public async Task<StatusMessage> ClearEmptySessions() {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.ClearEmptySessions()
        );
    }
    

    public async Task<DataActionResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetSessionsForProfile(profileID)
        );
    }
    
    //includes messages
    public async Task<DataActionResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetSessionReport(monitoringSessionID)
        );
    }

    //for input validation
    public async Task<DataActionResult<bool>> HasChildren(int nodeID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.HasChildren(nodeID)
        );
    }
    
    public async Task<DataActionResult<bool>> CheckIfNodeExists(int nodeID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfNodeExists(nodeID)
        );
    }
    
    public async Task<DataActionResult<bool>> CheckIfProfileExists(int profileID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfProfileExists(profileID)
        );
    }
    
    public async Task<DataActionResult<int>> GetCWSParamNumber(int cwsID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetCWSParamNumber(cwsID)
        );
    }

    public async Task<DataActionResult<bool>> CheckIfSessionExists(int sessionID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfSessionExists(sessionID)
        );
    }

    public async Task<DataActionResult<bool>> CheckIfCWSBindingExists(int cwsID, int nodeID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfCWSBindingExists(cwsID, nodeID)
        );
    }
    
    public async Task<DataActionResult<bool>> CheckIfTagExists(int tagID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfTagExists(tagID)
        );
    }

    public async Task<DataActionResult<bool>> CheckIfTagsExist(IEnumerable<int> tagsIDs) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfTagsExist(tagsIDs)
        );
    }
    
    public async Task<DataActionResult<bool>> CheckIfTagNameExists(string name, int? exceptID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfTagNameExists(name, exceptID)
        );
    }
    
    public async Task<DataActionResult<bool>> CheckIfNodeNameExists(string name, int? exceptID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfNodeNameExists(name, exceptID)
        );
    }
    
    public async Task<DataActionResult<bool>> CheckIfProfileNameExists(string name, int? exceptID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfProfileNameExists(name, exceptID)
        );
    }
    
    public async Task<DataActionResult<bool>> CheckIfCWSNameExists(string name, int? exceptID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfCWSNameExists(name, exceptID)
        );
    }
    
    public async Task<DataActionResult<bool>> CheckIfNodeInSubtree(int nodeID, int subtreeRootNodeID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CheckIfNodeInSubtree(nodeID, subtreeRootNodeID)
        );
    }
    

    public async Task<StatusMessage> MoveNodesSubtree(int nodeID, int newParentID) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.MoveNodesSubtree(nodeID, newParentID)
        );
    }
    
    public async Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles() {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetAllProfiles()
        );
    }
    
    public async Task<DataActionResult<Model.IntermediateModel.AllRawNodesData>> GetAllNodesData() {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetAllNodesData()
        );
    }

    public async Task<DataActionResult<uint>> GetNodeIP(int nodeID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetNodeIP(nodeID)
        );
    }
    
    public async Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetTaggedNodesIDs(tagID)
        );
    }
    
    public async Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetIDsOfNodesBySelectedTagsInProfileView(profileID)
        );
    }
    
    public async Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetIDsOfNodesBySelectedTagsInProfileMonitor(profileID)
        );
    }
    
    public async Task<DataActionResult<IEnumerable<NodeTag>>> GetAllTags() {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetAllTags()
        );
    }
    
    public async Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCWS() {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetAllCWS()
        );
    }

    public async Task<DataActionResult<string>> GetCWSBoundingString(
        int nodeID,
        int cwsID
    ) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.GetCWSBoundingString(nodeID, cwsID)
        );
    }
    
    

    public async Task<DataActionResult<Profile>> CreateProfile(Profile profile) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CreateProfile(profile)
        );
    }
    
    public async Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CreateNodeOnRoot(node)
        );
    }
    
    public async Task<DataActionResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CreateNodeWithParent(node, parentID)
        );
    }
    
    public async Task<DataActionResult<NodeTag>> CreateTag(NodeTag tag) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CreateTag(tag)
        );
    }
    
    public async Task<StatusMessage> CreateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3
    ) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.CreateWebServiceBinding(nodeID, cwsID, param1, param2, param3)
        );
    }
    
    public async Task<DataActionResult<CustomWebService>> CreateCustomWebService(CustomWebService cws) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.CreateCustomWebService(cws)
        );
    }

    public async Task<StatusMessage> SetNodeTags(int nodeID, IEnumerable<int> tagIDs) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.SetNodeTags(nodeID, tagIDs)
        );
    }
    
    public async Task<StatusMessage> SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.SetProfileViewTagsSelection(profileID, tagIDs)
        );
    }
    
    public async Task<StatusMessage> SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileID) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.SetProfileViewTagsSelectionToProfileMonitorTagsSelection(profileID)
        );
    }
    
    public async Task<StatusMessage> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.SetProfileMonitorTagsSelection(profileID, tagIDs)
        );
    }
    
    public async Task<StatusMessage> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.SetProfileMonitorTagsSelectionToProfileViewTagsSelection(profileID)
        );
    }
    

    public async Task<StatusMessage> UpdateProfile(Profile profile) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.UpdateProfile(profile)
        );
    }

    public async Task<StatusMessage> UpdateTag(NodeTag tag) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.UpdateTag(tag)
        );
    }
    
    public async Task<StatusMessage> UpdateNode(NtwkNode node) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.UpdateNode(node)
        );
    }
    
    public async Task<StatusMessage> UpdateCustomWebService(CustomWebService cws) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.UpdateCustomWebService(cws)
        );
    }
    
    public async Task<StatusMessage> UpdateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.UpdateWebServiceBinding(nodeID, cwsID, param1, param2, param3)
        );
    }
    

    public async Task<DataActionResult<Profile>> RemoveProfile(int profileID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.RemoveProfile(profileID)
        );
    }
    
    public async Task<DataActionResult<NtwkNode>> RemoveNode(int nodeID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.RemoveNode(nodeID)
        );
    }
    
    public async Task<DataActionResult<NodeTag>> RemoveTag(int tagID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.RemoveTag(tagID)
        );
    }
    
    public async Task<DataActionResult<CustomWebService>> RemoveCustomWebService(int cwsID) {
        return await PerformDataOperationOrLogExceptions(async () =>
            await efDataSource.RemoveCustomWebService(cwsID)
        );
    }
    
    public async Task<StatusMessage> RemoveWebServiceBinding(int nodeID, int cwsID) {
        return await PerformOperationOrLogExceptions(async () =>
            await efDataSource.RemoveWebServiceBinding(nodeID, cwsID)
        );
    }
}

}