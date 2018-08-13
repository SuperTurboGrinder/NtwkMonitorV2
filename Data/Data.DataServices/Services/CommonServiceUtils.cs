using System;
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

    async Task<string> ValidateID(
        Func<Task<DbOperationResult<bool>>> validator,
        string identity
    ) {
        DbOperationResult<bool> IDIsOk =
            await validator();
        if(!IDIsOk.Success) {
            return $"Unable to check existance of {identity} ID.";
        }
        else if(!IDIsOk.Result) {
            return $"Invalid {identity} id.";
        }
        return null;
    }

    async Task<string> ValidateMultipleID(
        Func<Task<DbOperationResult<bool>>> validator,
        string identity
    ) {
        DbOperationResult<bool> IDIsOk =
            await validator();
        if(!IDIsOk.Success) {
            return $"Unable to check existance of {identity} IDs.";
        }
        else if(!IDIsOk.Result) {
            return $"Invalid {identity} IDs.";
        }
        return null;
    }

    public async Task<string> ValidateProfileID(int profileID) {
        return await ValidateID(
            async () => await repo.CheckIfProfileExists(profileID),
            "profile"
        );
    }
    
    public async Task<string> ValidateSessionID(int sessionID) {
        return await ValidateID(
            async () => await repo.CheckIfSessionExists(sessionID),
            "session"
        );
    }

    public async Task<string> ValidateTagID(int tagID) {
        return await ValidateID(
            async () => await repo.CheckIfTagExists(tagID),
            "tag"
        );
    }

    public async Task<string> ValidateTagsIDs(IEnumerable<int> tagsIDs) {
        return await ValidateMultipleID(
            async () => await repo.CheckIfTagsExist(tagsIDs),
            "tags"
        );
    }

    public async Task<string> ValidateNodeID(int nodeID) {
        return await ValidateID(
            async () => await repo.CheckIfNodeExists(nodeID),
            "node"
        );
    }

    async Task<string> ErrorIfNameExists(Func<Task<DbOperationResult<bool>>> doesNameExist) {
        DbOperationResult<bool> nameExists =
            await doesNameExist();
        if(!nameExists.Success) {
            return "Unable to check existance of a name.";
        }
        else if(nameExists.Result) {
            return "Name already claimed.";
        }
        return null;
    }

    public async Task<string> ErrorIfNodeNameExists(string name) {
        return await ErrorIfNameExists(async () => 
            await repo.CheckIfNodeNameExists(name)
        );
    }

    public async Task<string> ErrorIfTagNameExists(string name) {
        return await ErrorIfNameExists(async () => 
            await repo.CheckIfTagNameExists(name)
        );
    }

    public async Task<string> ErrorIfCWSNameExists(string name) {
        return await ErrorIfNameExists(async () => 
            await repo.CheckIfCWSNameExists(name)
        );
    }

    public async Task<string> ErrorIfProfileNameExists(string name) {
        return await ErrorIfNameExists(async () => 
            await repo.CheckIfProfileNameExists(name)
        );
    }

    public async Task<string> ErrorIfCWSBindingExists(int cwsID, int nodeID) {
        DbOperationResult<bool> bindingExists =
            await repo.CheckIfCWSBindingExists(cwsID, nodeID);
        if(!bindingExists.Success) {
            return "Unable to check existance of a web service binding.";
        }
        else if(bindingExists.Result) {
            return "Binding between specified service and node already exists.";
        }
        return null;
    }

}

}