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

    public async Task<DataActionResult<AllNodesData>> GetAllNodesData() {
        DbOperationResult<Model.IntermediateModel.AllRawNodesData> dbOpResult =
            await repo.GetAllNodesData();
        if(!dbOpResult.Success) {
            return utils.FailActResult<AllNodesData>(
                "Unable to get all data for all nodes from database."
            );
        }
        return utils.SuccActResult(new AllNodesData() {
            WebServicesData = dbOpResult.Result.WebServicesData,
            NodesData = dbOpResult.Result.NodesData
                .Select(nd_group => nd_group
                    .Select(nd => new NodeData() {
                        Node = EFToViewConverter.Convert(nd.Node),
                        BoundWebServicesIDs = nd.BoundWebServicesIDs,
                        TagsIDs = nd.TagsIDs
                    })
                )
                .ToList()
        });
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
}

}