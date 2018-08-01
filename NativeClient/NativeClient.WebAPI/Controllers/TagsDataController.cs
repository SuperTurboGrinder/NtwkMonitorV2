using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;

namespace NativeClient.WebAPI.Controllers {

[Route("api/nodeTags")]
public class TagsDataController : BaseDataController {
    readonly ITagsDataService data;

    TagsDataController(ITagsDataService _data) {
        data = _data;
    }

    // GET api/nodeTags
    [HttpGet]
    public async Task<ActionResult> GetAllTags() {
        return await GetDbData(async () =>
            await data.GetAllTags()
        );
    }

    // POST api/nodeTags/new
    [HttpPost("/new")]
    public async Task<ActionResult> CreateTag(NodeTag tag) {
        return await GetDbData(async () =>
            await data.CreateTag(tag)
        );
    }

    // DELETE api/nodeTags/1/delete
    [HttpDelete("/{tagID:int}/delete")]
    public async Task<ActionResult> RemoveTag(int tagID) {
        return await GetDbData(async () =>
            await data.RemoveTag(tagID)
        );
    }
}

}