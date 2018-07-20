using System.Collections.Generic;
using System.Threading.Tasks;

using Data.Abstract.DbInteraction;
using Data.Abstract.DataAccessServices;

namespace Data.DataServices.Services {

class CommonServiceUtils {
    IDataRepository repo;

    public CommonServiceUtils(IDataRepository _repo) {
        repo = _repo;
    }

    
    public DataActionResult<T> SuccActResult<T>(T result) {
        return new DataActionResult<T>(result, true, null);
    }

    public DataActionResult<T> FailActResult<T>(string error) {
        return new DataActionResult<T>(default(T), false, error);
    }

    public DataActionVoidResult SuccActVoid() {
        return new DataActionVoidResult(true, null);
    }

    public DataActionVoidResult FailActVoid(string error) {
        return new DataActionVoidResult(false, error);
    }


    public async Task<string> ValidateProfileID(int profileID) {
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

    public async Task<string> ValidateTagsIDs(IEnumerable<int> tagsIDs) {
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
    
    public async Task<string> ValidateSessionID(int sessionID) {
        DbOperationResult<bool> SessionIDIsOk =
            await repo.CheckIfSessionExists(sessionID);
        if(!SessionIDIsOk.Success) {
            return "Unable to check existance of sessionID.";
        }
        else if(!SessionIDIsOk.Result) {
            return "Invalid session id.";
        }
        return null;
    }

}

}