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

namespace Data.DataServices.Services {

class SettingProfilesDataSerivce : ISettingsProfileDataService {
    readonly IDataRepository repo;
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;
    readonly CommonServiceUtils utils;

    public SettingProfilesDataSerivce(
        IDataRepository _repo,
        IViewModelValidator _validator,
        IViewModelToEFModelConverter _viewToEFConverter,
        IEFModelToViewModelConverter _EFToViewConverter
    ) {
        repo = _repo;
        validator = _validator;
        viewToEFConverter = _viewToEFConverter;
        EFToViewConverter = _EFToViewConverter;
        utils = new CommonServiceUtils(repo);
    }

    public async Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles() {
        DbOperationResult<IEnumerable<EFDbModel.Profile>> rawProfiles =
            await repo.GetAllProfiles();
        if(!rawProfiles.Success) {
            return utils.FailActResult<IEnumerable<Profile>>(
                "Unable to get profiles data from database."
            );
        }
        IEnumerable<Profile> converted = rawProfiles.Result
            .Select(rp => EFToViewConverter.Convert(rp));
        return utils.SuccActResult(converted);
        
    }

    public async Task<DataActionResult<Profile>> CreateProfile(Profile profile) {
        string errorStr = validator.Validate(profile);
        if(errorStr != null) {
            return utils.FailActResult<Profile>(errorStr);
        }
        EFDbModel.Profile converted = viewToEFConverter.Convert(profile);
        DbOperationResult<EFDbModel.Profile> dbOpResult =
            await repo.CreateProfile(converted);
        if(!dbOpResult.Success) {
            return utils.FailActResult<Profile>("Unable to save profile to database.");
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }

    public async Task<DataActionVoidResult> SetProfileViewTagsSelection(
        int profileID,
        IEnumerable<int> tagIDs
    ) {
        string pValidationError = await utils.ValidateProfileID(profileID);
        if(pValidationError != null) {
            return utils.FailActVoid(pValidationError);
        }
        string tValidationError = await utils.ValidateTagsIDs(tagIDs);
        if(tValidationError != null) {
            return utils.FailActVoid(tValidationError);
        }
        DbOperationVoidResult result = await repo.SetProfileViewTagsSelection(
            profileID,
            tagIDs
        );
        if(!result.Success) {
            return utils.FailActVoid("Database operation error while attempting to set profile view tags selection.");
        }
        return utils.SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(int profileID) {
        string pValidationError = await utils.ValidateProfileID(profileID);
        if(pValidationError != null) {
            return utils.FailActVoid(pValidationError);
        }
        DbOperationVoidResult result = await repo.SetProfileViewTagsSelectionToProfileMonitorTagsSelection(
            profileID
        );
        if(!result.Success) {
            return utils.FailActVoid("Database operation error while attempting to set profile view tags selection to monitor tags selection.");
        }
        return utils.SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        string pValidationError = await utils.ValidateProfileID(profileID);
        if(pValidationError != null) {
            return utils.FailActVoid(pValidationError);
        }
        string tValidationError = await utils.ValidateTagsIDs(tagIDs);
        if(tValidationError != null) {
            return utils.FailActVoid(tValidationError);
        }
        DbOperationVoidResult result = await repo.SetProfileMonitorTagsSelection(
            profileID,
            tagIDs
        );
        if(!result.Success) {
            return utils.FailActVoid("Database operation error while attempting to set profile monitor tags selection.");
        }
        return utils.SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID) {
        string pValidationError = await utils.ValidateProfileID(profileID);
        if(pValidationError != null) {
            return utils.FailActVoid(pValidationError);
        }
        DbOperationVoidResult result = await repo.SetProfileMonitorTagsSelectionToProfileViewTagsSelection(
            profileID
        );
        if(!result.Success) {
            return utils.FailActVoid("Database operation error while attempting to set monitor tags selection to profile view tags selection.");
        }
        return utils.SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> UpdateProfile(Profile profile) {
        string pValidationError = await utils.ValidateProfileID(profile.ID);
        if(pValidationError != null) {
            return utils.FailActVoid(pValidationError);
        }
        string errorStr = validator.Validate(profile);
        if(errorStr != null) {
            return utils.FailActVoid(errorStr);
        }
        EFDbModel.Profile converted = viewToEFConverter.Convert(profile);
        DbOperationVoidResult dbOpResult =
            await repo.UpdateProfile(converted);
        if(!dbOpResult.Success) {
            return utils.FailActVoid("Unable to save updated profile to database.");
        }
        return utils.SuccActVoid();
    }
    
    public async Task<DataActionResult<Profile>> RemoveProfile(int profileID) {
        string pValidationError = await utils.ValidateProfileID(profileID);
        if(pValidationError != null) {
            return utils.FailActResult<Profile>(pValidationError);
        }
        DbOperationResult<EFDbModel.Profile> dbOpResult =
            await repo.RemoveProfile(profileID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<Profile>("Unable to remove profile from database.");
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }
}

}