using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through IDataActionResult
public interface ISettingsProfileDataService {
    Task<IDataActionResult<bool>> CheckIfProfileExists(int profileID);
    Task<IDataActionResult<IEnumerable<Profile>>> GetAllProfiles();
    Task<IDataActionResult<Profile>> CreateProfile(Profile profile);
    Task<IDataActionVoidResult> SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<IDataActionVoidResult> SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(int profileID);
    Task<IDataActionVoidResult> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<IDataActionVoidResult> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID);
    Task<IDataActionVoidResult> UpdateProfile(Profile profile);
    Task<IDataActionResult<Profile>> RemoveProfile(int profileID);
}

}