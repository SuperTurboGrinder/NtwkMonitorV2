using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.DataAccessServices
{
//validation -> conversion -> DataActionResult
    public interface ITagsDataService
    {
        Task<DataActionResult<IEnumerable<NodeTag>>> GetAllTags();
        Task<DataActionResult<NodeTag>> CreateTag(NodeTag tag);
        Task<StatusMessage> UpdateTag(NodeTag tag);
        Task<DataActionResult<NodeTag>> RemoveTag(int tagId);
    }
}