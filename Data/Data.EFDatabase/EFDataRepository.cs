using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Abstract.DbInteraction;
using Data.Model.EFDbModel;
using Data.Model.ResultsModel;

namespace Data.EFDatabase
{
    public class EfDataRepository : IDataRepository
    {
        private readonly IEfDbDataSource _efDataSource;
        private readonly IDbErrorLogger _logger;

        public EfDataRepository(IEfDbDataSource dataSource, IDbErrorLogger logger)
        {
            _efDataSource = dataSource;
            _logger = logger;
        }

        async Task<DataActionResult<T>> PerformDataOperationOrLogExceptions<T>(
            Func<Task<T>> getter
        )
        {
            try
            {
                return DataActionResult<T>.Successful(await getter());
            }
            catch (AggregateException aggregateException)
            {
                aggregateException.Handle(ex =>
                {
                    _logger.LogException(ex);
                    return true;
                });
                return DataActionResult<T>.Failed(StatusMessage.DatabaseInternalError);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return DataActionResult<T>.Failed(StatusMessage.DatabaseInternalError);
            }
        }

        async Task<StatusMessage> PerformOperationOrLogExceptions(
            Func<Task> action
        )
        {
            try
            {
                await action();
                return StatusMessage.Ok;
            }
            catch (AggregateException aggregateException)
            {
                aggregateException.Handle(ex =>
                {
                    _logger.LogException(ex);
                    return true;
                });
                return StatusMessage.DatabaseInternalError;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return StatusMessage.DatabaseInternalError;
            }
        }

        public async Task<DataActionResult<MonitoringSession>> GetNewSession(
            int profileId
        )
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetNewSession(profileId)
            );
        }

        public async Task<DataActionResult<MonitoringPulseResult>> SavePulseResult(
            int sessionId,
            MonitoringPulseResult pulseResult,
            IEnumerable<MonitoringMessage> messages
        )
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.SavePulseResult(sessionId, pulseResult, messages)
            );
        }

        public async Task<StatusMessage> ClearEmptySessions()
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.ClearEmptySessions()
            );
        }


        public async Task<DataActionResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetSessionsForProfile(profileId)
            );
        }

        //includes messages
        public async Task<DataActionResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(
            int monitoringSessionId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetSessionReport(monitoringSessionId)
            );
        }

        //for input validation
        public async Task<DataActionResult<bool>> HasChildren(int nodeId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.HasChildren(nodeId)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfNodeExists(int nodeId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfNodeExists(nodeId)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfProfileExists(int profileId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfProfileExists(profileId)
            );
        }

        public async Task<DataActionResult<int>> GetCwsParamNumber(int cwsId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetCwsParamNumber(cwsId)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfSessionExists(int sessionId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfSessionExists(sessionId)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfCwsBindingExists(int cwsId, int nodeId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfCwsBindingExists(cwsId, nodeId)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfTagExists(int tagId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfTagExists(tagId)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfTagsExist(IEnumerable<int> tagsIDs)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfTagsExist(tagsIDs)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfTagNameExists(string name, int? exceptId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfTagNameExists(name, exceptId)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfNodeNameExists(string name, int? exceptId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfNodeNameExists(name, exceptId)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfProfileNameExists(string name, int? exceptId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfProfileNameExists(name, exceptId)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfCwsNameExists(string name, int? exceptId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfCwsNameExists(name, exceptId)
            );
        }

        public async Task<DataActionResult<bool>> CheckIfNodeInSubtree(int nodeId, int subtreeRootNodeId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CheckIfNodeInSubtree(nodeId, subtreeRootNodeId)
            );
        }


        public async Task<StatusMessage> MoveNodesSubtree(int nodeId, int newParentId)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.MoveNodesSubtree(nodeId, newParentId)
            );
        }

        public async Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles()
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetAllProfiles()
            );
        }

        public async Task<DataActionResult<Model.IntermediateModel.AllRawNodesData>> GetAllNodesData()
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetAllNodesData()
            );
        }

        public async Task<DataActionResult<uint>> GetNodeIp(int nodeId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetNodeIp(nodeId)
            );
        }

        public async Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetTaggedNodesIDs(tagId)
            );
        }

        public async Task<DataActionResult<Model.ViewModel.TagFilterData>> GetProfileViewTagFilterData(int profileId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetProfileViewTagFilterData(profileId)
            );
        }

        public async Task<DataActionResult<Model.ViewModel.TagFilterData>> GetProfileMonitorTagFilterData(int profileId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetProfileMonitorTagFilterData(profileId)
            );
        }

        public async Task<DataActionResult<IEnumerable<NodeTag>>> GetAllTags()
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetAllTags()
            );
        }

        public async Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCws()
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetAllCws()
            );
        }

        public async Task<DataActionResult<string>> GetCwsBoundingString(
            int nodeId,
            int cwsId
        )
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.GetCwsBoundingString(nodeId, cwsId)
            );
        }


        public async Task<DataActionResult<Profile>> CreateProfile(Profile profile)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CreateProfile(profile)
            );
        }

        public async Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CreateNodeOnRoot(node)
            );
        }

        public async Task<DataActionResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CreateNodeWithParent(node, parentId)
            );
        }

        public async Task<DataActionResult<NodeTag>> CreateTag(NodeTag tag)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CreateTag(tag)
            );
        }

        public async Task<StatusMessage> CreateWebServiceBinding(int nodeId, int cwsId,
            string param1, string param2, string param3
        )
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.CreateWebServiceBinding(nodeId, cwsId, param1, param2, param3)
            );
        }

        public async Task<DataActionResult<CustomWebService>> CreateCustomWebService(CustomWebService cws)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.CreateCustomWebService(cws)
            );
        }

        public async Task<StatusMessage> SetNodeTags(int nodeId, IEnumerable<int> tagIDs)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.SetNodeTags(nodeId, tagIDs)
            );
        }

        public async Task<StatusMessage> SetProfileViewTagsSelection(int profileId, IEnumerable<int> tagIDs)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.SetProfileViewTagsSelection(profileId, tagIDs)
            );
        }

        public async Task<StatusMessage> SetProfileViewTagsSelectionToProfileMonitorTagsSelection(int profileId)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.SetProfileViewTagsSelectionToProfileMonitorTagsSelection(profileId)
            );
        }

        public async Task<StatusMessage> SetProfileMonitorTagsSelection(int profileId, IEnumerable<int> tagIDs)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.SetProfileMonitorTagsSelection(profileId, tagIDs)
            );
        }

        public async Task<StatusMessage> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileId)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.SetProfileMonitorTagsSelectionToProfileViewTagsSelection(profileId)
            );
        }


        public async Task<StatusMessage> UpdateProfile(Profile profile)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.UpdateProfile(profile)
            );
        }

        public async Task<StatusMessage> UpdateTag(NodeTag tag)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.UpdateTag(tag)
            );
        }

        public async Task<StatusMessage> UpdateNode(NtwkNode node)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.UpdateNode(node)
            );
        }

        public async Task<StatusMessage> UpdateCustomWebService(CustomWebService cws)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.UpdateCustomWebService(cws)
            );
        }

        public async Task<StatusMessage> UpdateWebServiceBinding(int nodeId, int cwsId,
            string param1, string param2, string param3)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.UpdateWebServiceBinding(nodeId, cwsId, param1, param2, param3)
            );
        }


        public async Task<DataActionResult<Profile>> RemoveProfile(int profileId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.RemoveProfile(profileId)
            );
        }

        public async Task<DataActionResult<NtwkNode>> RemoveNode(int nodeId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.RemoveNode(nodeId)
            );
        }

        public async Task<DataActionResult<NodeTag>> RemoveTag(int tagId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.RemoveTag(tagId)
            );
        }

        public async Task<DataActionResult<CustomWebService>> RemoveCustomWebService(int cwsId)
        {
            return await PerformDataOperationOrLogExceptions(async () =>
                await _efDataSource.RemoveCustomWebService(cwsId)
            );
        }

        public async Task<StatusMessage> RemoveWebServiceBinding(int nodeId, int cwsId)
        {
            return await PerformOperationOrLogExceptions(async () =>
                await _efDataSource.RemoveWebServiceBinding(nodeId, cwsId)
            );
        }
    }
}