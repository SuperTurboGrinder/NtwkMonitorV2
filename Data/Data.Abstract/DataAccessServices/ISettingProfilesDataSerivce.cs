using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.DataAccessServices
{
//validation -> conversion -> DataActionResult
    public interface ISettingsProfileDataService
    {
        Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles();
        Task<DataActionResult<TagFilterData>> GetProfileViewTagFilterData(int profileId);
        Task<DataActionResult<TagFilterData>> GetProfileMonitorTagFilterData(int profileId);
        Task<DataActionResult<Profile>> CreateProfile(Profile profile);
        Task<StatusMessage> SetProfileViewTagsSelection(int profileId, IEnumerable<int> tagIDs);
        Task<StatusMessage> SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(int profileId);
        Task<StatusMessage> SetProfileMonitorTagsSelection(int profileId, IEnumerable<int> tagIDs);
        Task<StatusMessage> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileId);
        Task<StatusMessage> UpdateProfile(Profile profile);
        Task<DataActionResult<Profile>> RemoveProfile(int profileId);
    }
}