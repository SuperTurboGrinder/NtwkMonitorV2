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

class NodeTreeDataService : INodeTreeDataService {
    readonly IDataRepository repo;
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;
    readonly CommonServiceUtils utils;

    public NodeTreeDataService(
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

    public async Task<DataActionResult<IEnumerable<NtwkNode>>> GetAllNodes() {
        DbOperationResult<IEnumerable<EFDbModel.NtwkNode>> dbOpResult =
            await repo.GetAllNodes();
        if(!dbOpResult.Success) {
            return utils.FailActResult<IEnumerable<NtwkNode>>(
                "Unable to get all nodes list from database."
            );
        }
        return utils.SuccActResult(dbOpResult.Result
            .Select(n => EFToViewConverter.Convert(n))
        );
    }
    
    public async Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(
        int tagID
    ) {
        string tValidationError = await utils.ValidateTagID(tagID);
        if(tValidationError != null) {
            return utils.FailActResult<IEnumerable<int>>(tValidationError);
        }
        DbOperationResult<IEnumerable<int>> dbOpResult =
            await repo.GetTaggedNodesIDs(tagID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<IEnumerable<int>>(
                "Unable to get tagged nodes list from database."
            );
        }
        return utils.SuccActResult(dbOpResult.Result);
    }
    
    public async Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileView(int profileID) {
        string pValidationError = await utils.ValidateProfileID(profileID);
        if(pValidationError != null) {
            return utils.FailActResult<IEnumerable<int>>(pValidationError);
        }
        DbOperationResult<IEnumerable<int>> dbOpResult =
            await repo.GetIDsOfNodesBySelectedTagsInProfileView(profileID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<IEnumerable<int>>(
                "Unable to get profile view nodes IDs from database."
            );
        }
        return utils.SuccActResult(dbOpResult.Result);
    }
    
    public async Task<DataActionResult<IEnumerable<int>>> GetIDsOfNodesBySelectedTagsInProfileMonitor(int profileID) {
        string pValidationError = await utils.ValidateProfileID(profileID);
        if(pValidationError != null) {
            return utils.FailActResult<IEnumerable<int>>(pValidationError);
        }
        DbOperationResult<IEnumerable<int>> dbOpResult =
            await repo.GetIDsOfNodesBySelectedTagsInProfileMonitor(profileID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<IEnumerable<int>>(
                "Unable to get profile monitor nodes IDs from database."
            );
        }
        return utils.SuccActResult(dbOpResult.Result);
    }
    
    public async Task<DataActionResult<IEnumerable<NodeTag>>> GetAllTags() {
        DbOperationResult<IEnumerable<EFDbModel.NodeTag>> dbOpResult =
            await repo.GetAllTags();
        if(!dbOpResult.Success) {
            return utils.FailActResult<IEnumerable<NodeTag>>(
                "Unable to get all tags list from database."
            );
        }
        return utils.SuccActResult(dbOpResult.Result
            .Select(t => EFToViewConverter.Convert(t))
        );
    }
    
    public async Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCWS() {
        DbOperationResult<IEnumerable<EFDbModel.CustomWebService>> dbOpResult =
            await repo.GetAllCWS();
        if(!dbOpResult.Success) {
            return utils.FailActResult<IEnumerable<CustomWebService>>(
                "Unable to get all CWS list from database."
            );
        }
        return utils.SuccActResult(dbOpResult.Result
            .Select(cws => EFToViewConverter.Convert(cws))
        );
    }
    
    

    public async Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node) {
        string errorStr = validator.Validate(node);
        if(errorStr != null) {
            return utils.FailActResult<NtwkNode>(errorStr);
        }
        string nameExistsError = await utils.ErrorIfNodeNameExists(node.Name);
        if(nameExistsError != null) {
            return utils.FailActResult<NtwkNode>(nameExistsError);
        }
        DbOperationResult<EFDbModel.NtwkNode> dbOpResult =
            await repo.CreateNodeOnRoot(viewToEFConverter.Convert(node));
        if(!dbOpResult.Success) {
            return utils.FailActResult<NtwkNode>(
                "Unable to create network node in database."
            );
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }
    
    public async Task<DataActionResult<NtwkNode>> CreateNodeWithParent(
        NtwkNode node,
        int parentID
    ) {
        string pValidationError = await utils.ValidateNodeID(parentID);
        if(pValidationError != null) {
            return utils.FailActResult<NtwkNode>(pValidationError);
        }
        string errorStr = validator.Validate(node);
        if(errorStr != null) {
            return utils.FailActResult<NtwkNode>(errorStr);
        }
        string nameExistsError = await utils.ErrorIfNodeNameExists(node.Name);
        if(nameExistsError != null) {
            return utils.FailActResult<NtwkNode>(nameExistsError);
        }
        DbOperationResult<EFDbModel.NtwkNode> dbOpResult =
            await repo.CreateNodeWithParent(viewToEFConverter.Convert(node), parentID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<NtwkNode>(
                "Unable to create network node in database."
            );
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }
    
    public async Task<DataActionResult<NodeTag>> CreateTag(NodeTag tag) {
        string errorStr = validator.Validate(tag);
        if(errorStr != null) {
            return utils.FailActResult<NodeTag>(errorStr);
        }
        string nameExistsError = await utils.ErrorIfTagNameExists(tag.Name);
        if(nameExistsError != null) {
            return utils.FailActResult<NodeTag>(nameExistsError);
        }
        DbOperationResult<EFDbModel.NodeTag> dbOpResult =
            await repo.CreateTag(viewToEFConverter.Convert(tag));
        if(!dbOpResult.Success) {
            return utils.FailActResult<NodeTag>(
                "Unable to create tag in database."
            );
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }

    async Task<DataActionResult<int>> GetCWSParamNumber(int cwsID) {
        DbOperationResult<int> paramNum =
            await repo.GetCWSParamNumber(cwsID);
        if(!paramNum.Success) {
            return utils.FailActResult<int>("Unable to get CWS parameter number from database.");
        }
        else if(paramNum.Result == -1) {
            return utils.FailActResult<int>("Invalid CWS ID.");
        }
        else return utils.SuccActResult(paramNum.Result);
    }

    async Task<DataActionVoidResult> CWSBindingValidationRoutine(
        int nodeID,
        int cwsID,
        string param1,
        string param2,
        string param3
    ) {
        string nValidationError = await utils.ValidateNodeID(nodeID);
        if(nValidationError != null) {
            return utils.FailActVoid(nValidationError);
        }
        DataActionResult<int> paramNum = await GetCWSParamNumber(cwsID);
        if(!paramNum.Success) {
            return utils.FailActVoid(paramNum.Error);
        }
        int n = paramNum.Result;
        string paramNumValidation = (
            (n != 0 && param1 == null) ||
            (n > 1 && param2 == null) ||
            (n == 3 && param3 == null)
        ) ? (
            "Service binding set parameter value can not be null."
        ) : (
                (n < 3 && param3 != null) ||
                (n < 2 && param2 != null) ||
                (n == 0 && param1 != null)
            ) ? (
                "Redundant parameters values in service binding."
            ) : null;
        if(paramNumValidation != null) {
            return utils.FailActVoid(paramNumValidation);
        }
        return utils.SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> CreateWebServiceBinding(
        int nodeID,
        int cwsID,
        string param1,
        string param2,
        string param3
    ) {
        DataActionVoidResult cwsWalidation = await CWSBindingValidationRoutine(
            nodeID, cwsID, param1, param2, param3
        );
        if(!cwsWalidation.Success) {
            return utils.FailActVoid(cwsWalidation.Error);
        }
        DbOperationVoidResult dbOpResult =
            await repo.CreateWebServiceBinding(nodeID, cwsID, param1, param2, param3);
        if(!dbOpResult.Success) {
            return utils.FailActVoid(
                "Unable to create web service binding in database."
            );
        }
        return utils.SuccActVoid();
    }
    
    public async Task<DataActionResult<CustomWebService>> CreateCustomWebService(
        CustomWebService cws
    ) {
        string errorStr = validator.Validate(cws);
        if(errorStr != null) {
            return utils.FailActResult<CustomWebService>(errorStr);
        }
        string nameExistsError = await utils.ErrorIfCWSNameExists(cws.ServiceName);
        if(nameExistsError != null) {
            return utils.FailActResult<CustomWebService>(nameExistsError);
        }
        DbOperationResult<EFDbModel.CustomWebService> dbOpResult =
            await repo.CreateCustomWebService(viewToEFConverter.Convert(cws));
        if(!dbOpResult.Success) {
            return utils.FailActResult<CustomWebService>(
                "Unable to create custom web service in database."
            );
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }

    

    public async Task<DataActionVoidResult> MoveNodesSubtree(
        int nodeID,
        int newParentID
    ) {
        string nValidationError = await utils.ValidateNodeID(nodeID);
        if(nValidationError != null) {
            return utils.FailActVoid(nValidationError);
        }
        nValidationError = await utils.ValidateNodeID(newParentID);
        if(nValidationError != null) {
            return utils.FailActVoid(nValidationError);
        }
        DbOperationResult<bool> dbOpResult =
            await repo.CheckIfNodeInSubtree(newParentID, nodeID);
        if(!dbOpResult.Success) {
            return utils.FailActVoid(
                "Unable to verify if new parent is not in nodes subtree."
            );
        }
        if(dbOpResult.Result) {
            return utils.FailActVoid(
                "New subtree parent can not be a part of the same subtree."
            );
        }
        DbOperationVoidResult dbSubtreeMoveOpResult =
            await repo.MoveNodesSubtree(nodeID, newParentID);
        if(!dbSubtreeMoveOpResult.Success) {
            return utils.FailActVoid(
                "Unable to move subtree of nodes in database."
            );
        }
        return utils.SuccActVoid();
    }

    public async Task<DataActionVoidResult> SetNodeTags(
        int nodeID,
        IEnumerable<int> tagIDs
    ) {
        string nValidationError = await utils.ValidateNodeID(nodeID);
        if(nValidationError != null) {
            return utils.FailActVoid(nValidationError);
        }
        string tValidationError = await utils.ValidateTagsIDs(tagIDs);
        if(tValidationError != null) {
            return utils.FailActVoid(tValidationError);
        }
        DbOperationVoidResult dbOpResult =
            await repo.SetNodeTags(nodeID, tagIDs);
        if(!dbOpResult.Success) {
            return utils.FailActVoid(
                "Unable to set node tags in database."
            );
        }
        return utils.SuccActVoid();
    }
    

    public async Task<DataActionVoidResult> UpdateNode(
        NtwkNode node
    ) {
        string nValidationError = await utils.ValidateNodeID(node.ID);
        if(nValidationError != null) {
            return utils.FailActVoid(nValidationError);
        }
        string errorStr = validator.Validate(node);
        if(errorStr != null) {
            return utils.FailActVoid(errorStr);
        }
        string nameExistsError = await utils.ErrorIfNodeNameExists(node.Name);
        if(nameExistsError != null) {
            return utils.FailActVoid(nameExistsError);
        }
        DbOperationVoidResult dbOpResult =
            await repo.UpdateNode(viewToEFConverter.Convert(node));
        if(!dbOpResult.Success) {
            return utils.FailActVoid(
                "Unable to update node in database."
            );
        }
        return utils.SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> UpdateCustomWebService(
        CustomWebService cws
    ) {
        DataActionResult<int> paramNum = await GetCWSParamNumber(cws.ID);
        if(!paramNum.Success) {
            return utils.FailActVoid(paramNum.Error);
        }
        string errorStr = validator.Validate(cws);
        if(errorStr != null) {
            return utils.FailActVoid(errorStr);
        }
        string nameExistsError = await utils.ErrorIfNodeNameExists(cws.ServiceName);
        if(nameExistsError != null) {
            return utils.FailActVoid(nameExistsError);
        }
        DbOperationVoidResult dbOpResult =
            await repo.UpdateCustomWebService(viewToEFConverter.Convert(cws));
        if(!dbOpResult.Success) {
            return utils.FailActVoid(
                "Unable to update custom web service in database."
            );
        }
        return utils.SuccActVoid();
    }
    
    public async Task<DataActionVoidResult> UpdateWebServiceBinding(
        int nodeID,
        int cwsID,
        string param1,
        string param2,
        string param3
    ) {
        DataActionVoidResult cwsWalidation = await CWSBindingValidationRoutine(
            nodeID, cwsID, param1, param2, param3
        );
        if(!cwsWalidation.Success) {
            return utils.FailActVoid(cwsWalidation.Error);
        }
        DbOperationVoidResult dbOpResult =
            await repo.UpdateWebServiceBinding(nodeID, cwsID, param1, param2, param3);
        if(!dbOpResult.Success) {
            return utils.FailActVoid(
                "Unable to update web service binding in database."
            );
        }
        return utils.SuccActVoid();
    }
    

    public async Task<DataActionResult<NtwkNode>> RemoveNode(int nodeID) {
        string nValidationError = await utils.ValidateNodeID(nodeID);
        if(nValidationError != null) {
            return utils.FailActResult<NtwkNode>(nValidationError);
        }
        DbOperationResult<EFDbModel.NtwkNode> dbOpResult =
            await repo.RemoveNode(nodeID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<NtwkNode>(
                "Unable to remove network node from database."
            );
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }

    public async Task<DataActionResult<NodeTag>> RemoveTag(int tagID) {
        string nValidationError = await utils.ValidateTagID(tagID);
        if(nValidationError != null) {
            return utils.FailActResult<NodeTag>(nValidationError);
        }
        DbOperationResult<EFDbModel.NodeTag> dbOpResult =
            await repo.RemoveTag(tagID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<NodeTag>(
                "Unable to remove tag from database."
            );
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }

    public async Task<DataActionResult<CustomWebService>> RemoveCustomWebService(
        int cwsID
    ) {
        DataActionResult<int> paramNum = await GetCWSParamNumber(cwsID);
        if(!paramNum.Success) {
            return utils.FailActResult<CustomWebService>(paramNum.Error);
        }
        DbOperationResult<EFDbModel.CustomWebService> dbOpResult =
            await repo.RemoveCustomWebService(cwsID);
        if(!dbOpResult.Success) {
            return utils.FailActResult<CustomWebService>(
                "Unable to remove custom web service from database."
            );
        }
        return utils.SuccActResult(EFToViewConverter.Convert(dbOpResult.Result));
    }
    
    public async Task<DataActionVoidResult> RemoveWebServiceBinding(
        int nodeID,
        int cwsID
    ) {
        string nValidationError = await utils.ValidateNodeID(nodeID);
        if(nValidationError != null) {
            return utils.FailActVoid(nValidationError);
        }
        DataActionResult<int> paramNum = await GetCWSParamNumber(cwsID);
        if(!paramNum.Success) {
            return utils.FailActVoid(paramNum.Error);
        }
        DbOperationResult<EFDbModel.CustomWebService> dbOpResult =
            await repo.RemoveCustomWebService(cwsID);
        if(!dbOpResult.Success) {
            return utils.FailActVoid(
                "Unable to remove web service binding from database."
            );
        }
        return utils.SuccActVoid();
    }
}

}