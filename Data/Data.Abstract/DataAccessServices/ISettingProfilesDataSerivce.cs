using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through DataActionResult
public interface ISettingsProfileDataService {
    Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles();
    Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID);
    Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID);
    Task<DataActionResult<Profile>> CreateProfile(Profile profile);
    Task<DataActionVoidResult> SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<DataActionVoidResult> SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(int profileID);
    Task<DataActionVoidResult> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<DataActionVoidResult> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID);
    Task<DataActionVoidResult> UpdateProfile(Profile profile);
    Task<DataActionResult<Profile>> RemoveProfile(int profileID);
}

}