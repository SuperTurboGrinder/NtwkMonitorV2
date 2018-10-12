using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using NativeClient.WebAPI.Abstract;
using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;

namespace NativeClient.WebAPI.Controllers {

[Route("api/nodeTags")]
public class TagsDataController : BaseDataController {
    readonly ITagsDataService data;

    public TagsDataController(
        ITagsDataService _data,
        IErrorReportAssemblerService _errAssembler
    ) : base(_errAssembler) {
        data = _data;
    }

    // GET api/nodeTags
    [HttpGet]
    public async Task<ActionResult> GetAllTags() {
        return ObserveDataOperationResult(
            await data.GetAllTags()
        );
    }

    // POST api/nodeTags/new
    [HttpPost("new")]
    public async Task<ActionResult> CreateTag([FromBody] NodeTag tag) {
        return ObserveDataOperationResult(
            await data.CreateTag(tag)
        );
    }

    //PUT api/nodeTags/1/update
    [HttpPut("{tagID:int}/update")]
    public async Task<ActionResult> UpdateTag(int tagID, [FromBody] NodeTag tag) {
        tag.ID = tagID;
        return ObserveDataOperationStatus(
            await data.UpdateTag(tag)
        );
    }

    // DELETE api/nodeTags/1/delete
    [HttpDelete("{tagID:int}/delete")]
    public async Task<ActionResult> RemoveTag(int tagID) {
        return ObserveDataOperationResult(
            await data.RemoveTag(tagID)
        );
    }
}

}