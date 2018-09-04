using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Data.Abstract.DbInteraction;
using Data.Abstract.DataAccessServices;
using Data.Model.ResultsModel;

namespace Data.DataServices.Services {

public abstract class BaseDataService {
    protected IDataRepository repo;

    protected BaseDataService(IDataRepository _repo) {
        repo = _repo;
    }

    StatusMessage IDsAreOk(
        DataActionResult<bool> validatorResult,
        StatusMessage invalidIDsMessage
    ) {
        return validatorResult.Status.Failure()
            ? StatusMessage.ErrorWhileCheckingIDInDatabase
            : validatorResult.Result == false
                ? invalidIDsMessage
                : StatusMessage.Ok;
    }

    protected async Task<StatusMessage> ValidateProfileID(int profileID) {
        return IDsAreOk(
            await repo.CheckIfProfileExists(profileID),
            StatusMessage.InvalidProfileID
        );
    }
    
    protected async Task<StatusMessage> ValidateSessionID(int sessionID) {
        return IDsAreOk(
            await repo.CheckIfSessionExists(sessionID),
            StatusMessage.InvalidSessionID
        );
    }

    protected async Task<StatusMessage> ValidateTagID(int tagID) {
        return IDsAreOk(
            await repo.CheckIfTagExists(tagID),
            StatusMessage.InvalidTagID
        );
    }

    protected async Task<StatusMessage> ValidateNodeID(int nodeID) {
        return IDsAreOk(
            await repo.CheckIfNodeExists(nodeID),
            StatusMessage.InvalidNodeID
        );
    }

    protected async Task<StatusMessage> ValidateTagsIDs(IEnumerable<int> tagsIDs) {
        return IDsAreOk(
            await repo.CheckIfTagsExist(tagsIDs),
            StatusMessage.InvalidTagsIDs
        );
    }

    StatusMessage FailIfNameExists(DataActionResult<bool> nameExistsResult) {
        return nameExistsResult.Status.Failure()
            ? StatusMessage.ErrorWhileCheckingNameExistanceInDatabase
            : nameExistsResult.Result == true
                ? StatusMessage.NameAlreadyClaimed
                : StatusMessage.Ok;
    }

    protected async Task<StatusMessage> FailIfNodeNameExists(string name) {
        return FailIfNameExists(await repo.CheckIfNodeNameExists(name));
    }

    protected async Task<StatusMessage> FailIfTagNameExists(string name) {
        return FailIfNameExists(await repo.CheckIfTagNameExists(name));
    }

    protected async Task<StatusMessage> FailIfCWSNameExists(string name) {
        return FailIfNameExists(await repo.CheckIfCWSNameExists(name));
    }

    protected async Task<StatusMessage> FailIfProfileNameExists(string name) {
        return FailIfNameExists(await repo.CheckIfProfileNameExists(name));
    }

    protected async Task<StatusMessage> FailIfCWSBindingExists(int cwsID, int nodeID) {
        DataActionResult<bool> bindingExists =
            await repo.CheckIfCWSBindingExists(nodeID, cwsID);
        return bindingExists.Status.Failure()
            ? StatusMessage.ErrorWhileGetingExistanceOfWSBindingFromDatabase
            : bindingExists.Result == true
                ? StatusMessage.BindingBetweenSpecifiedServiceAndNodeAlreadyExists
                : StatusMessage.Ok;
    }

    protected async Task<StatusMessage> FailIfCWSBinding_DOES_NOT_Exist(int cwsID, int nodeID) {
        DataActionResult<bool> bindingExists =
            await repo.CheckIfCWSBindingExists(nodeID, cwsID);
        return bindingExists.Status.Failure()
            ? StatusMessage.ErrorWhileGetingExistanceOfWSBindingFromDatabase
            : bindingExists.Result == false
                ? StatusMessage.BindingBetweenSpecifiedServiceAndNodeDoesNotExist
                : StatusMessage.Ok;
    }

    protected async Task<StatusMessage> FailIfNodeInSubtree(
        int nodeID,
        int subtreeRootNodeID
    ) {
        DataActionResult<bool> nodeInSubtree =
            await repo.CheckIfNodeInSubtree(nodeID, subtreeRootNodeID);
        return nodeInSubtree.Status.Failure()
            ? StatusMessage.ErrorWhileCheckingIfNodeIsPartOfSubtree
            : nodeInSubtree.Result == true
                ? StatusMessage.NodeIsPartOfSpecifiedSubtree
                : StatusMessage.Ok;
    }

    protected async Task<DataActionResult<int>> GetCWSParamNumber(int cwsID) {
        DataActionResult<int> paramNumResult =
            await repo.GetCWSParamNumber(cwsID);
        return paramNumResult.Status.Failure()
            ? DataActionResult<int>.Failed(StatusMessage.ErrorWhileCheckingCWSParamNumberInDatabase)
            : paramNumResult.Result == -1
                ? DataActionResult<int>.Failed(StatusMessage.InvalidWebServiceID)
                : paramNumResult;
    }

    protected DataActionResult<TModelOut> FailOrConvert<TModelIn, TModelOut>(
        DataActionResult<TModelIn> input,
        Func<TModelIn, TModelOut> convert
    ) {
        return input.Status.Failure()
            ? DataActionResult<TModelOut>.Failed(input.Status)
            : DataActionResult<TModelOut>.Successful(convert(input.Result));
    }

}

}