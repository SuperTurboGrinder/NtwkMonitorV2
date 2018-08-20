using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through DataActionResult
public interface ISettingsProfileDataService {
    Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles();
    Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID);
    Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID);
    Task<DataActionResult<Profile>> CreateProfile(Profile profile);
    Task<StatusMessage> SetProfileViewTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<StatusMessage> SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(int profileID);
    Task<StatusMessage> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs);
    Task<StatusMessage> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID);
    Task<StatusMessage> UpdateProfile(Profile profile);
    Task<DataActionResult<Profile>> RemoveProfile(int profileID);
}

}