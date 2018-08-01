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

    SettingsProfileDataController(ISettingsProfileDataService _data) {
        data = _data;
    }

    // GET api/settingsProfiles
    [HttpGet]
    public async Task<ActionResult> GetAllProfiles() {
        return await GetDbData(async () =>
            await data.GetAllProfiles()
        );
    }
    
    // GET api/settingsProfiles/mainViewNodesIDs
    [HttpGet("/mainViewNodesIDs")]
    public async Task<ActionResult> GetIDsOfNodesBySelectedTagsInProfileView(int profileID) {
        return await GetDbData(async () =>
            await data.GetIDsOfNodesBySelectedTagsInProfileView(profileID)
        );
    }

    // GET api/settingsProfiles/monitorNodesIDs
    [HttpGet("/monitorNodesIDs")]
    public async Task<ActionResult> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID) {
        return await GetDbData(async () =>
            await data.GetIDsOfNodesBySelectedTagsInProfileMonitor(profileID)
        );
    }

    // POST api/settingsProfiles/new
    [HttpPost("/new")]
    public async Task<ActionResult> CreateProfile(Profile profile) {
        return await GetDbData(async () =>
            await data.CreateProfile(profile)
        );
    }

    // POST api/settingsProfiles/1/setViewTags
    [HttpGet("/{profileID:int}/setViewTags")]
    public async Task<ActionResult> SetProfileViewTagsSelection(
        int profileID,
        IEnumerable<int> tagIDs
    ) {
        return await PerformDBOperation(async () =>
            await data.SetProfileViewTagsSelection(profileID, tagIDs)
        );
    }

    // POST api/settingsProfiles/1/setMonitorTags
    [HttpGet("/{profileID:int}/setMonitorTags")]
    public async Task<ActionResult> SetProfileMonitorTagsSelection(
        int profileID,
        IEnumerable<int> tagIDs
    ) {
        return await PerformDBOperation(async () =>
            await data.SetProfileMonitorTagsSelection(profileID, tagIDs)
        );
    }

    // POST api/settingsProfiles/1/setViewTagsToMonitorTags
    [HttpGet("/{profileID:int}/setViewTagsToMonitorTags")]
    public async Task<ActionResult> SetProfileViewTagsToMonitorTags(int profileID) {
        return await PerformDBOperation(async () =>
            await data.SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(profileID)
        );
    }

    // POST api/settingsProfiles/1/setMonitorTagsToViewTags
    [HttpGet("/{profileID:int}/setMonitorTagsToViewTags")]
    public async Task<ActionResult> SetProfileMonitorTagsToViewTags(int profileID) {
        return await PerformDBOperation(async () =>
            await data.SetProfileMonitorTagsSelectionToProfileViewTagsSelection(profileID)
        );
    }

    // POST api/settingsProfiles/update/1
    [HttpPost("/update/{profileID:int}")]
    public async Task<ActionResult> UpdateProfile(int profileID, Profile profile) {
        profile.ID = profileID;
        return await PerformDBOperation(async () =>
            await data.UpdateProfile(profile)
        );
    }

    // POST api/settingsProfiles/remove/1
    [HttpPost("/remove/{profileID:int}")]
    public async Task<ActionResult> RemoveProfile(int profileID) {
        return await GetDbData(async () =>
            await data.RemoveProfile(profileID)
        );
    }
}

}