using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Abstract.DataAccessServices;
using Data.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Controllers
{
    [Route("api/settingsProfiles")]
    public class SettingsProfileDataController : BaseDataController
    {
        readonly ISettingsProfileDataService _data;

        public SettingsProfileDataController(
            ISettingsProfileDataService data,
            IErrorReportAssemblerService errAssembler
        ) : base(errAssembler)
        {
            _data = data;
        }

        // GET api/settingsProfiles
        [HttpGet]
        public async Task<ActionResult> GetAllProfiles()
        {
            return ObserveDataOperationResult(
                await _data.GetAllProfiles()
            );
        }

        // GET api/settingsProfiles/1/mainViewTagFilterData
        [HttpGet("{profileID:int}/mainViewTagFilterData")]
        public async Task<ActionResult> GetProfileViewTagFilterData(int profileId)
        {
            return ObserveDataOperationResult(
                await _data.GetProfileViewTagFilterData(profileId)
            );
        }

        // GET api/settingsProfiles/1/monitorTagFilterData
        [HttpGet("{profileID:int}/monitorTagFilterData")]
        public async Task<ActionResult> GetProfileMonitorTagFilterData(int profileId)
        {
            return ObserveDataOperationResult(
                await _data.GetProfileMonitorTagFilterData(profileId)
            );
        }

        // POST api/settingsProfiles/new
        [HttpPost("new")]
        public async Task<ActionResult> CreateProfile([FromBody] Profile profile)
        {
            return ObserveDataOperationResult(
                await _data.CreateProfile(profile)
            );
        }

        // POST api/settingsProfiles/1/setViewTags
        [HttpPost("{profileID:int}/setViewTags")]
        public async Task<ActionResult> SetProfileViewTagsSelection(
            int profileId,
            [FromBody] IEnumerable<int> tagIDs
        )
        {
            return ObserveDataOperationStatus(
                await _data.SetProfileViewTagsSelection(profileId, tagIDs)
            );
        }

        // POST api/settingsProfiles/1/setMonitorTags
        [HttpPost("{profileID:int}/setMonitorTags")]
        public async Task<ActionResult> SetProfileMonitorTagsSelection(
            int profileId,
            [FromBody] IEnumerable<int> tagIDs
        )
        {
            return ObserveDataOperationStatus(
                await _data.SetProfileMonitorTagsSelection(profileId, tagIDs)
            );
        }

        // PUT api/settingsProfiles/1/setViewTagsToMonitorTags
        [HttpPut("{profileID:int}/setViewTagsToMonitorTags")]
        public async Task<ActionResult> SetProfileViewTagsToMonitorTags(int profileId)
        {
            return ObserveDataOperationStatus(
                await _data.SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(profileId)
            );
        }

        // PUT api/settingsProfiles/1/setMonitorTagsToViewTags
        [HttpPut("{profileID:int}/setMonitorTagsToViewTags")]
        public async Task<ActionResult> SetProfileMonitorTagsToViewTags(int profileId)
        {
            return ObserveDataOperationStatus(
                await _data.SetProfileMonitorTagsSelectionToProfileViewTagsSelection(profileId)
            );
        }

        // PUT api/settingsProfiles/1/update
        [HttpPut("{profileID:int}/update")]
        public async Task<ActionResult> UpdateProfile(int profileId, [FromBody] Profile profile)
        {
            profile.Id = profileId;
            return ObserveDataOperationStatus(
                await _data.UpdateProfile(profile)
            );
        }

        // DELETE api/settingsProfiles/1/remove
        [HttpDelete("{profileID:int}/remove")]
        public async Task<ActionResult> RemoveProfile(int profileId)
        {
            return ObserveDataOperationResult(
                await _data.RemoveProfile(profileId)
            );
        }
    }
}