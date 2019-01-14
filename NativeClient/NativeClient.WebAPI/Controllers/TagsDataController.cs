using System.Threading.Tasks;
using Data.Abstract.DataAccessServices;
using Data.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Controllers
{
    [Route("api/nodeTags")]
    public class TagsDataController : BaseDataController
    {
        readonly ITagsDataService _data;

        public TagsDataController(
            ITagsDataService data,
            IErrorReportAssemblerService errAssembler
        ) : base(errAssembler)
        {
            _data = data;
        }

        // GET api/nodeTags
        [HttpGet]
        public async Task<ActionResult> GetAllTags()
        {
            return ObserveDataOperationResult(
                await _data.GetAllTags()
            );
        }

        // POST api/nodeTags/new
        [HttpPost("new")]
        public async Task<ActionResult> CreateTag([FromBody] NodeTag tag)
        {
            return ObserveDataOperationResult(
                await _data.CreateTag(tag)
            );
        }

        //PUT api/nodeTags/1/update
        [HttpPut("{tagID:int}/update")]
        public async Task<ActionResult> UpdateTag(int tagId, [FromBody] NodeTag tag)
        {
            tag.Id = tagId;
            return ObserveDataOperationStatus(
                await _data.UpdateTag(tag)
            );
        }

        // DELETE api/nodeTags/1/delete
        [HttpDelete("{tagID:int}/delete")]
        public async Task<ActionResult> RemoveTag(int tagId)
        {
            return ObserveDataOperationResult(
                await _data.RemoveTag(tagId)
            );
        }
    }
}