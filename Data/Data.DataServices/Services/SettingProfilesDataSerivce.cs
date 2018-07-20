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
    readonly IDataRopository repo;
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;

    public SettingProfilesDataSerivce(
        IDataRopository _repo,
        IViewModelValidator _validator,
        IViewModelToEFModelConverter _viewToEFConverter,
        IEFModelToViewModelConverter _EFToViewConverter
    ) {
        repo = _repo;
        validator = _validator;
        viewToEFConverter = _viewToEFConverter;
        EFToViewConverter = _EFToViewConverter;
    }

    DataActionResult<T> SuccActResult<T>(T result) {
        return new DataActionResult<T>(result, true, null);
    }

    DataActionResult<T> FailActResult<T>(string error) {
        return new DataActionResult<T>(default(T), false, error);
    }

    DataActionVoidResult SuccActVoid() {
        return new DataActionVoidResult(true, null);
    }

    DataActionVoidResult FailActVoid(string error) {
        return new DataActionVoidResult(false, error);
    }

    public async Task<DataActionResult<IEnumerable<Profile>>> GetAllProfiles() {
        DbOperationResult<IEnumerable<EFDbModel.Profile>> rawProfiles =
            await repo.GetAllProfiles();
        if(!rawProfiles.Success) {
            return FailActResult<IEnumerable<Profile>>(
                "Unable to get profiles data from database."
            );
        }
        IEnumerable<Profile> converted = rawProfiles.Result
            .Select(rp => EFToViewConverter.Convert(rp));
        return SuccActResult(converted);
        
    }

    public async Task<DataActionResult<Profile>> CreateProfile(Profile profile) {
        string errorStr = validator.Validate(profile);
        if(errorStr != null) {
            return FailActResult<Profile>(errorStr);
        }
        EFDbModel.Profile converted = viewToEFConverter.Convert(profile);
        DbOperationResult<EFDbModel.Profile> dbOpResult =
            await repo.CreateProfile(converted);
        if(!dbOpResult.Success) {
            return FailActResult<Profile>("Unable to save profile to database.");
        }
        return SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }

    async Task<string> ValidateProfileID(int profileID) {
        DbOperationResult<bool> ProfileIDIsOk =
            await repo.CheckIfProfileExists(profileID);
        if(!ProfileIDIsOk.Success) {
            return "Unable to check existance of profileID.";
        }
        else if(!ProfileIDIsOk.Result) {
            return "Invalid profile id.";
        }
        return null;
    }

    async Task<string> ValidateTagsIDs(IEnumerable<int> tagsIDs) {
        DbOperationResult<bool> TagIDsAreOk =
            await repo.CheckIfTagsExist(tagsIDs);
        if(!TagIDsAreOk.Success) {
            return "Unable to check existance of tag IDs.";
        }
        else if(!TagIDsAreOk.Result) {
            return "Invalid tag IDs.";
        }
        return null;
    }
    
    public async Task<DataActionVoidResult> SetProfileViewTagsSelection(
        int profileID,
        IEnumerable<int> tagIDs
    ) {
        string pValidationError = await ValidateProfileID(profileID);
        if(pValidationError != null) {
            return FailActVoid(pValidationError);
        }
        string tValidationError = await ValidateTagsIDs(tagIDs);
        if(tValidationError != null) {
            return FailActVoid(tValidationError);
        }
        DbOperationVoidResult result = await repo.SetProfileViewTagsSelection(
            profileID,
            tagIDs
        );
        if(!result.Success) {
            return FailActVoid("Database operation error while attempting to set profile view tags selection.");
        }
        return SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> SetProfileViewTagsSelectionToProfileMonitorFlagsSelection(int profileID) {
        string pValidationError = await ValidateProfileID(profileID);
        if(pValidationError != null) {
            return FailActVoid(pValidationError);
        }
        DbOperationVoidResult result = await repo.SetProfileViewTagsSelectionToProfileMonitorTagsSelection(
            profileID
        );
        if(!result.Success) {
            return FailActVoid("Database operation error while attempting to set profile view tags selection to monitor tags selection.");
        }
        return SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> SetProfileMonitorTagsSelection(int profileID, IEnumerable<int> tagIDs) {
        string pValidationError = await ValidateProfileID(profileID);
        if(pValidationError != null) {
            return FailActVoid(pValidationError);
        }
        string tValidationError = await ValidateTagsIDs(tagIDs);
        if(tValidationError != null) {
            return FailActVoid(tValidationError);
        }
        DbOperationVoidResult result = await repo.SetProfileMonitorTagsSelection(
            profileID,
            tagIDs
        );
        if(!result.Success) {
            return FailActVoid("Database operation error while attempting to set profile monitor tags selection.");
        }
        return SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> SetProfileMonitorTagsSelectionToProfileViewTagsSelection(int profileID) {
        string pValidationError = await ValidateProfileID(profileID);
        if(pValidationError != null) {
            return FailActVoid(pValidationError);
        }
        DbOperationVoidResult result = await repo.SetProfileMonitorTagsSelectionToProfileViewTagsSelection(
            profileID
        );
        if(!result.Success) {
            return FailActVoid("Database operation error while attempting to set monitor tags selection to profile view tags selection.");
        }
        return SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> UpdateProfile(Profile profile) {
        string pValidationError = await ValidateProfileID(profile.ID);
        if(pValidationError != null) {
            return FailActVoid(pValidationError);
        }
        string errorStr = validator.Validate(profile);
        if(errorStr != null) {
            return FailActVoid(errorStr);
        }
        EFDbModel.Profile converted = viewToEFConverter.Convert(profile);
        DbOperationVoidResult dbOpResult =
            await repo.UpdateProfile(converted);
        if(!dbOpResult.Success) {
            return FailActVoid("Unable to save updated profile to database.");
        }
        return SuccActVoid();
    }
    
    public async Task<DataActionResult<Profile>> RemoveProfile(int profileID) {
        string pValidationError = await ValidateProfileID(profileID);
        if(pValidationError != null) {
            return FailActResult<Profile>(pValidationError);
        }
        DbOperationResult<EFDbModel.Profile> dbOpResult =
            await repo.RemoveProfile(profileID);
        if(!dbOpResult.Success) {
            return FailActResult<Profile>("Unable to remove profile from database.");
        }
        return SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }
}

}