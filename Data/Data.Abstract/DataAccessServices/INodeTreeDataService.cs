using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.DataAccessServices
{
//validation -> conversion -> DataActionResult
    public interface INodeTreeDataService
    {
        Task<DataActionResult<AllNodesData>> GetAllNodesData();
        Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(int tagId);

        Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node);
        Task<DataActionResult<NtwkNode>> CreateNodeWithParent(NtwkNode node, int parentId);

        Task<StatusMessage> SetNodeTags(int nodeId, IEnumerable<int> tagIDs);
        Task<StatusMessage> MoveNodesSubtree(int nodeId, int newParentId);

        Task<StatusMessage> UpdateNode(NtwkNode node);
        Task<DataActionResult<NtwkNode>> RemoveNode(int nodeId);
    }
}