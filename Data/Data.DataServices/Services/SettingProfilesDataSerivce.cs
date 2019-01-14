using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Data.Abstract.DataAccessServices;
using Data.Abstract.DbInteraction;
using Data.Abstract.Validation;
using Data.Abstract.Converters;
using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.DataServices.Services
{
    public class SettingsProfilesDataService
        : BaseDataService, ISettingsProfileDataService
    {
        readonly IViewModelValidator _validator;
        readonly IViewModelToEfModelConverter _viewToEfConverter;
        readonly IEfModelToViewModelConverter _efToViewConverter;

        public SettingsProfilesDataService(
            IDataRepository repo,
            IViewModelValidator validator,
            IViewModelToEfModelConverter viewToEfConverter,
            IEfModelToViewModelConverter efToViewConverter
        ) : base(repo)
        {
            _validator = validator;
            _viewToEfConverter = viewToEfConverter;
            _efToViewConverter = efToViewConverter;
        }

        public async Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles()
        {
            return FailOrConvert(
                await Repo.GetAllProfiles(),
                profiles => profiles.Select(p => _efToViewConverter.Convert(p))
            );
        }

        public async Task<DataActionResult<TagFilterData>> GetProfileViewTagFilterData(int profileId)
        {
            StatusMessage profileIdValidationStatus = await ValidateProfileId(profileId);
            if (profileIdValidationStatus.Failure())
            {
                return DataActionResult<TagFilterData>.Failed(profileIdValidationStatus);
            }

            return await Repo.GetProfileViewTagFilterData(profileId);
        }

        public async Task<DataActionResult<TagFilterData>> GetProfileMonitorTagFilterData(int profileId)
        {
            StatusMessage profileIdValidationStatus = await ValidateProfileId(profileId);
            if (profileIdValidationStatus.Failure())
            {
                return DataActionResult<TagFilterData>.Failed(profileIdValidationStatus);
            }

            return await Repo.GetProfileMonitorTagFilterData(profileId);
        }

        public async Task<DataActionResult<Profile>> CreateProfile(Profile profile)
        {
            StatusMessage profileValidationStatus = _validator.Validate(profile);
            if (profileValidationStatus.Failure())
            {
                return DataActionResult<Profile>.Failed(profileValidationStatus);
            }

            StatusMessage nameExistsStatus = await FailIfProfileNameExists(profile.Name);
            if (nameExistsStatus.Failure())
            {
                return DataActionResult<Profile>.Failed(nameExistsStatus);
            }

            return FailOrConvert(
                await Repo.CreateProfile(_viewToEfConverter.Convert(profile)),
                p => _efToViewConverter.Convert(p)
            );
        }

        public async Task<StatusMessage> SetProfileViewTagsSelection(
            int profileId,
            IEnumerable<int> tagIDs
        )
        {
            StatusMessage profileIdValidationStatus = await ValidateProfileId(profileId);
            if (profileIdValidationStatus.Failure())
            {
                return profileIdValidationStatus;
            }

            IEnumerable<int> tagsIDs = tagIDs as int[] ?? tagIDs.ToArray();
            StatusMessage tagsIDsValidationStatus = await ValidateTagsIDs(tagsIDs);
            if (tagsIDsValidationStatus.Failure())
            {
                return tagsIDsValidationStatus;
            }

            return await Repo.SetProfileViewTagsSelection(profileId, tagsIDs);
        }

        public async Task<StatusMessage> SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(int profileId)
        {
            StatusMessage profileIdValidationStatus = await ValidateProfileId(profileId);
            if (profileIdValidationStatus.Failure())
            {
                return profileIdValidationStatus;
            }

            return await Repo.SetProfileViewTagsSelectionToProfileMonitorTagsSelection(
                profileId
            );
        }

        public async Task<StatusMessage> SetProfileMonitorTagsSelection(int profileId, IEnumerable<int> tagIDs)
        {
            StatusMessage profileIdValidationStatus = await ValidateProfileId(profileId);
            if (profileIdValidationStatus.Failure())
            {
                return profileIdValidationStatus;
            }

            IEnumerable<int> tagsIDs = tagIDs as int[] ?? tagIDs.ToArray();
            StatusMessage tagsIDsValidationStatus = await ValidateTagsIDs(tagsIDs);
            if (tagsIDsValidationStatus.Failure())
            {
                return tagsIDsValidationStatus;
            }

            return await Repo.SetProfileMonitorTagsSelection(profileId, tagsIDs);
        }

        public async Task<StatusMessage> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileId)
        {
            StatusMessage profileIdValidationStatus = await ValidateProfileId(profileId);
            if (profileIdValidationStatus.Failure())
            {
                return profileIdValidationStatus;
            }

            return await Repo.SetProfileMonitorTagsSelectionToProfileViewTagsSelection(
                profileId
            );
        }

        public async Task<StatusMessage> UpdateProfile(Profile profile)
        {
            StatusMessage profileIdValidationStatus = await ValidateProfileId(profile.Id);
            if (profileIdValidationStatus.Failure())
            {
                return profileIdValidationStatus;
            }

            StatusMessage profileValidationStatus = _validator.Validate(profile);
            if (profileValidationStatus.Failure())
            {
                return profileValidationStatus;
            }

            StatusMessage nameExistsStatus =
                await FailIfProfileNameExists(profile.Name, updatingProfileId: profile.Id);
            if (nameExistsStatus.Failure())
            {
                return nameExistsStatus;
            }

            return await Repo.UpdateProfile(_viewToEfConverter.Convert(profile));
        }

        public async Task<DataActionResult<Profile>> RemoveProfile(int profileId)
        {
            StatusMessage profileIdValidationStatus = await ValidateProfileId(profileId);
            if (profileIdValidationStatus.Failure())
            {
                return DataActionResult<Profile>.Failed(profileIdValidationStatus);
            }

            return FailOrConvert(
                await Repo.RemoveProfile(profileId),
                p => _efToViewConverter.Convert(p)
            );
        }
    }
}