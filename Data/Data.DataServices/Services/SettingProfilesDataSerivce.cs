using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Data.Abstract.DataAccessServices;
using Data.Abstract.DbInteraction;
using Data.Abstract.Validation;
using Data.Abstract.Converters;
using Data.Model.ViewModel;
using EFDbModel = Data.Model.EFDbModel;
using Data.Model.ResultsModel;

namespace Data.DataServices.Services {

public class SettingsProfilesDataService
    : BaseDataService, ISettingsProfileDataService {
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;

    public SettingsProfilesDataService(
        IDataRepository _repo,
        IViewModelValidator _validator,
        IViewModelToEFModelConverter _viewToEFConverter,
        IEFModelToViewModelConverter _EFToViewConverter
    ) : base(_repo) {
        validator = _validator;
        viewToEFConverter = _viewToEFConverter;
        EFToViewConverter = _EFToViewConverter;
    }

    public async Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles() {
        return FailOrConvert(
            await repo.GetAllProfiles(),
            profiles => profiles.Select(p => EFToViewConverter.Convert(p))
        );
    }
    
    public async Task<DataActionResult<TagFilterData>> GetProfileViewTagFilterData(int profileID) {
        StatusMessage profileIDValidationStatus =await ValidateProfileID(profileID);
        if(profileIDValidationStatus.Failure()) {
            return DataActionResult<TagFilterData>.Failed(profileIDValidationStatus);
        }
        return await repo.GetProfileViewTagFilterData(profileID);
    }
    
    public async Task<DataActionResult<TagFilterData>> GetProfileMonitorTagFilterData(int profileID) {
        StatusMessage profileIDValidationStatus =await ValidateProfileID(profileID);
        if(profileIDValidationStatus.Failure()) {
            return DataActionResult<TagFilterData>.Failed(profileIDValidationStatus);
        }
        return await repo.GetProfileMonitorTagFilterData(profileID);
    }

    public async Task<DataActionResult<Profile>> CreateProfile(Profile profile) {
        StatusMessage profileValidationStatus = validator.Validate(profile);
        if(profileValidationStatus.Failure()) {
            return DataActionResult<Profile>.Failed(profileValidationStatus);
        }
        StatusMessage nameExistsStatus = await FailIfProfileNameExists(profile.Name);
        if(nameExistsStatus.Failure()) {
            return DataActionResult<Profile>.Failed(nameExistsStatus);
        }
        return FailOrConvert(
            await repo.CreateProfile(viewToEFConverter.Convert(profile)),
            p => EFToViewConverter.Convert(p)
        );
    }

    public async Task<StatusMessage> SetProfileViewTagsSelection(
        int profileID,
        IEnumerable<int> tagIDs
    ) {
        StatusMessage profileIDValidationStatus =await ValidateProfileID(profileID);
        if(profileIDValidationStatus.Failure()) {
            return profileIDValidationStatus;
        }
        StatusMessage tagsIDsValidationStatus = await ValidateTagsIDs(tagIDs);
        if(tagsIDsValidationStatus.Failure()) {
            return tagsIDsValidationStatus;
        }
        return await repo.SetProfileViewTagsSelection(profileID, tagIDs);
    }
    
    public async Task<StatusMessage> SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(int profileID) {
        StatusMessage profileIDValidationStatus =await ValidateProfileID(profileID);
        if(profileIDValidationStatus.Failure()) {
            return profileIDValidationStatus;
        }
        return await repo.SetProfileViewTagsSelectionToProfileMonitorTagsSelection(
            profileID
        );
    }
    
    public async Task<StatusMessage> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        StatusMessage profileIDValidationStatus =await ValidateProfileID(profileID);
        if(profileIDValidationStatus.Failure()) {
            return profileIDValidationStatus;
        }
        StatusMessage tagsIDsValidationStatus = await ValidateTagsIDs(tagIDs);
        if(tagsIDsValidationStatus.Failure()) {
            return tagsIDsValidationStatus;
        }
        return await repo.SetProfileMonitorTagsSelection(profileID, tagIDs);
    }
    
    public async Task<StatusMessage> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID) {
        StatusMessage profileIDValidationStatus =await ValidateProfileID(profileID);
        if(profileIDValidationStatus.Failure()) {
            return profileIDValidationStatus;
        }
        return await repo.SetProfileMonitorTagsSelectionToProfileViewTagsSelection(
            profileID
        );
    }
    
    public async Task<StatusMessage> UpdateProfile(Profile profile) {
        StatusMessage profileIDValidationStatus = await ValidateProfileID(profile.ID);
        if(profileIDValidationStatus.Failure()) {
            return profileIDValidationStatus;
        }
        StatusMessage profileValidationStatus = validator.Validate(profile);
        if(profileValidationStatus.Failure()) {
            return profileValidationStatus;
        }
        StatusMessage nameExistsStatus =
            await FailIfProfileNameExists(profile.Name, updatingProfileID: profile.ID);
        if(nameExistsStatus.Failure()) {
            return nameExistsStatus;
        }
        return await repo.UpdateProfile(viewToEFConverter.Convert(profile));
    }
    
    public async Task<DataActionResult<Profile>> RemoveProfile(int profileID) {
        StatusMessage profileIDValidationStatus =await ValidateProfileID(profileID);
        if(profileIDValidationStatus.Failure()) {
            return DataActionResult<Profile>.Failed(profileIDValidationStatus);
        }
        return FailOrConvert(
            await repo.RemoveProfile(profileID),
            p => EFToViewConverter.Convert(p)
        );
    }
}

}