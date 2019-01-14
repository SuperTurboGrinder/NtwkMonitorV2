using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Abstract.DbInteraction;
using Data.Model.ResultsModel;

namespace Data.DataServices.Services
{
    public abstract class BaseDataService
    {
        protected readonly IDataRepository Repo;

        protected BaseDataService(IDataRepository repo)
        {
            Repo = repo;
        }

        StatusMessage DsAreOk(
            DataActionResult<bool> validatorResult,
            StatusMessage invalidIDsMessage
        )
        {
            return validatorResult.Status.Failure()
                ? StatusMessage.ErrorWhileCheckingIdInDatabase
                : validatorResult.Result == false
                    ? invalidIDsMessage
                    : StatusMessage.Ok;
        }

        protected async Task<StatusMessage> ValidateProfileId(int profileId)
        {
            return DsAreOk(
                await Repo.CheckIfProfileExists(profileId),
                StatusMessage.InvalidProfileId
            );
        }

        protected async Task<StatusMessage> ValidateSessionId(int sessionId)
        {
            return DsAreOk(
                await Repo.CheckIfSessionExists(sessionId),
                StatusMessage.InvalidSessionId
            );
        }

        protected async Task<StatusMessage> ValidateTagId(int tagId)
        {
            return DsAreOk(
                await Repo.CheckIfTagExists(tagId),
                StatusMessage.InvalidTagId
            );
        }

        protected async Task<StatusMessage> ValidateNodeId(int nodeId)
        {
            return DsAreOk(
                await Repo.CheckIfNodeExists(nodeId),
                StatusMessage.InvalidNodeId
            );
        }

        protected async Task<StatusMessage> ValidateTagsIDs(IEnumerable<int> tagsIDs)
        {
            return DsAreOk(
                await Repo.CheckIfTagsExist(tagsIDs),
                StatusMessage.InvalidTagsIDs
            );
        }

        StatusMessage FailIfNameExists(DataActionResult<bool> nameExistsResult)
        {
            return nameExistsResult.Status.Failure()
                ? StatusMessage.ErrorWhileCheckingNameExistanceInDatabase
                : nameExistsResult.Result
                    ? StatusMessage.NameAlreadyClaimed
                    : StatusMessage.Ok;
        }

        protected async Task<StatusMessage> FailIfNodeNameExists(
            string name,
            int? updatingNodeId = null
        )
        {
            return FailIfNameExists(
                await Repo.CheckIfNodeNameExists(name, updatingNodeId)
            );
        }

        protected async Task<StatusMessage> FailIfTagNameExists(
            string name,
            int? updatingTagId = null
        )
        {
            return FailIfNameExists(
                await Repo.CheckIfTagNameExists(name, updatingTagId)
            );
        }

        protected async Task<StatusMessage> FailIfCwsNameExists(
            string name,
            int? updatingCwsId = null
        )
        {
            return FailIfNameExists(
                await Repo.CheckIfCwsNameExists(name, updatingCwsId)
            );
        }

        protected async Task<StatusMessage> FailIfProfileNameExists(
            string name,
            int? updatingProfileId = null
        )
        {
            return FailIfNameExists(
                await Repo.CheckIfProfileNameExists(name, updatingProfileId)
            );
        }

        protected async Task<StatusMessage> FailIfCwsBindingExists(int cwsId, int nodeId)
        {
            DataActionResult<bool> bindingExists =
                await Repo.CheckIfCwsBindingExists(nodeId, cwsId);
            return bindingExists.Status.Failure()
                ? StatusMessage.ErrorWhileGetingExistanceOfWsBindingFromDatabase
                : bindingExists.Result
                    ? StatusMessage.BindingBetweenSpecifiedServiceAndNodeAlreadyExists
                    : StatusMessage.Ok;
        }

        protected async Task<StatusMessage> FailIfCWSBinding_DOES_NOT_Exist(int cwsId, int nodeId)
        {
            DataActionResult<bool> bindingExists =
                await Repo.CheckIfCwsBindingExists(nodeId, cwsId);
            return bindingExists.Status.Failure()
                ? StatusMessage.ErrorWhileGetingExistanceOfWsBindingFromDatabase
                : bindingExists.Result == false
                    ? StatusMessage.BindingBetweenSpecifiedServiceAndNodeDoesNotExist
                    : StatusMessage.Ok;
        }

        protected async Task<StatusMessage> FailIfNodeInSubtree(
            int nodeId,
            int subtreeRootNodeId
        )
        {
            DataActionResult<bool> nodeInSubtree =
                await Repo.CheckIfNodeInSubtree(nodeId, subtreeRootNodeId);
            return nodeInSubtree.Status.Failure()
                ? StatusMessage.ErrorWhileCheckingIfNodeIsPartOfSubtree
                : nodeInSubtree.Result
                    ? StatusMessage.NodeIsPartOfSpecifiedSubtree
                    : StatusMessage.Ok;
        }

        protected async Task<DataActionResult<int>> GetCwsParamNumber(int cwsId)
        {
            DataActionResult<int> paramNumResult =
                await Repo.GetCwsParamNumber(cwsId);
            return paramNumResult.Status.Failure()
                ? DataActionResult<int>.Failed(StatusMessage.ErrorWhileCheckingCwsParamNumberInDatabase)
                : paramNumResult.Result == -1
                    ? DataActionResult<int>.Failed(StatusMessage.InvalidWebServiceId)
                    : paramNumResult;
        }

        protected DataActionResult<TModelOut> FailOrConvert<TModelIn, TModelOut>(
            DataActionResult<TModelIn> input,
            Func<TModelIn, TModelOut> convert
        )
        {
            return input.Status.Failure()
                ? DataActionResult<TModelOut>.Failed(input.Status)
                : DataActionResult<TModelOut>.Successful(convert(input.Result));
        }
    }
}