using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;

namespace NativeClient.WebAPI.Controllers {

[Route("api/settingsProfiles")]
public class SettingsProfileDataController : BaseDataController {
    readonly ISettingsProfileDataService data;

    public SettingsProfileDataController(ISettingsProfileDataService _data) {
        data = _data;
    }

    // GET api/settingsProfiles
    [HttpGet]
    public async Task<ActionResult> GetAllProfiles() {
        return await GetDbData(async () =>
            await data.GetAllProfiles()
        );
    }
    
    // GET api/settingsProfiles/1/mainViewNodesIDs
    [HttpGet("{profileID:int}/mainViewNodesIDs")]
    public async Task<ActionResult> GetIDsOfNodesBySelectedTagsInProfileView(int profileID) {
        return await GetDbData(async () =>
            await data.GetIDsOfNodesBySelectedTagsInProfileView(profileID)
        );
    }

    // GET api/settingsProfiles/1/monitorNodesIDs
    [HttpGet("{profileID:int}/monitorNodesIDs")]
    public async Task<ActionResult> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID) {
        return await GetDbData(async () =>
            await data.GetIDsOfNodesBySelectedTagsInProfileMonitor(profileID)
        );
    }

    // POST api/settingsProfiles/new
    [HttpPost("new")]
    public async Task<ActionResult> CreateProfile([FromBody] Profile profile) {
        return await GetDbData(async () =>
            await data.CreateProfile(profile)
        );
    }

    // POST api/settingsProfiles/1/setViewTags
    [HttpPost("{profileID:int}/setViewTags")]
    public async Task<ActionResult> SetProfileViewTagsSelection(
        int profileID,
        [FromBody] IEnumerable<int> tagIDs
    ) {
        return await PerformDBOperation(async () =>
            await data.SetProfileViewTagsSelection(profileID, tagIDs)
        );
    }

    // POST api/settingsProfiles/1/setMonitorTags
    [HttpPost("{profileID:int}/setMonitorTags")]
    public async Task<ActionResult> SetProfileMonitorTagsSelection(
        int profileID,
        [FromBody] IEnumerable<int> tagIDs
    ) {
        return await PerformDBOperation(async () =>
            await data.SetProfileMonitorTagsSelection(profileID, tagIDs)
        );
    }

    // PUT api/settingsProfiles/1/setViewTagsToMonitorTags
    [HttpPut("{profileID:int}/setViewTagsToMonitorTags")]
    public async Task<ActionResult> SetProfileViewTagsToMonitorTags(int profileID) {
        return await PerformDBOperation(async () =>
            await data.SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(profileID)
        );
    }

    // PUT api/settingsProfiles/1/setMonitorTagsToViewTags
    [HttpPut("{profileID:int}/setMonitorTagsToViewTags")]
    public async Task<ActionResult> SetProfileMonitorTagsToViewTags(int profileID) {
        return await PerformDBOperation(async () =>
            await data.SetProfileMonitorTagsSelectionToProfileViewTagsSelection(profileID)
        );
    }

    // POST api/settingsProfiles/1/update
    [HttpPost("{profileID:int}/update")]
    public async Task<ActionResult> UpdateProfile(int profileID, [FromBody] Profile profile) {
        profile.ID = profileID;
        return await PerformDBOperation(async () =>
            await data.UpdateProfile(profile)
        );
    }

    // DELETE api/settingsProfiles/1/remove
    [HttpDelete("{profileID:int}/remove")]
    public async Task<ActionResult> RemoveProfile(int profileID) {
        return await GetDbData(async () =>
            await data.RemoveProfile(profileID)
        );
    }
}

}