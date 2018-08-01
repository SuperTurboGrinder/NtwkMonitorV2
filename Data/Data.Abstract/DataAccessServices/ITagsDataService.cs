using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through DataActionResult
public interface ITagsDataService {
    Task<DataActionResult<IEnumerable<NodeTag>>> GetAllTags();
    Task<DataActionResult<NodeTag>> CreateTag(NodeTag tag);
    Task<DataActionResult<NodeTag>> RemoveTag(int tagID);
}

}