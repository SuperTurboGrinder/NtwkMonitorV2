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

public class NodeTreeDataService
    : BaseDataService, INodeTreeDataService {
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;

    public NodeTreeDataService(
        IDataRepository _repo,
        IViewModelValidator _validator,
        IViewModelToEFModelConverter _viewToEFConverter,
        IEFModelToViewModelConverter _EFToViewConverter
    ) : base(_repo) {
        validator = _validator;
        viewToEFConverter = _viewToEFConverter;
        EFToViewConverter = _EFToViewConverter;
    }

    public async Task<DataActionResult<AllNodesData>> GetAllNodesData() {
        return FailOrConvert(
            await repo.GetAllNodesData(),
            nodesData => new AllNodesData() {
                WebServicesData = nodesData.WebServicesData,
                NodesData = nodesData.NodesData
                    .Select(nd_group => nd_group
                        .Select(nd => new NodeData() {
                            Node = EFToViewConverter.Convert(nd.Node),
                            BoundWebServicesIDs = nd.BoundWebServicesIDs,
                            TagsIDs = nd.TagsIDs
                        })
                    )
                    .ToList()
            }
        );
    }
    
    public async Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(
        int tagID
    ) {
        StatusMessage tagIDValidationStatus = await ValidateTagID(tagID);
        if(tagIDValidationStatus.Failure()) {
            return DataActionResult<IEnumerable<int>>.Failed(tagIDValidationStatus);
        }
        return await repo.GetTaggedNodesIDs(tagID);
    }
    
    
    public async Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node) {
        StatusMessage nodeValidationStatus = validator.Validate(node);
        if(nodeValidationStatus.Failure()) {
            return DataActionResult<NtwkNode>.Failed(nodeValidationStatus);
        }
        StatusMessage nameExistsStatus = await FailIfNodeNameExists(node.Name);
        if(nameExistsStatus.Failure()) {
            return DataActionResult<NtwkNode>.Failed(nameExistsStatus);
        }
        return FailOrConvert(
            await repo.CreateNodeOnRoot(viewToEFConverter.Convert(node)),
            n => EFToViewConverter.Convert(n)
        );
    }
    
    public async Task<DataActionResult<NtwkNode>> CreateNodeWithParent(
        NtwkNode node,
        int parentID
    ) {
        StatusMessage nodeIDValidationStatus = await ValidateNodeID(parentID);
        if(nodeIDValidationStatus.Failure()) {
            return DataActionResult<NtwkNode>.Failed(nodeIDValidationStatus);
        }
        StatusMessage nodeValidationStatus = validator.Validate(node);
        if(nodeValidationStatus.Failure()) {
            return DataActionResult<NtwkNode>.Failed(nodeValidationStatus);
        }
        StatusMessage nameExistsStatus = await FailIfNodeNameExists(node.Name);
        if(nameExistsStatus.Failure()) {
            return DataActionResult<NtwkNode>.Failed(nameExistsStatus);
        }
        return FailOrConvert(
            await repo.CreateNodeWithParent(viewToEFConverter.Convert(node), parentID),
            n => EFToViewConverter.Convert(n)
        );
    }

    

    public async Task<StatusMessage> MoveNodesSubtree(
        int nodeID,
        int newParentID
    ) {
        StatusMessage nodeIDValidationStatus = await ValidateNodeID(nodeID);
        if(nodeIDValidationStatus.Failure()) {
            return nodeIDValidationStatus;
        }
        StatusMessage newParentIDValidationStatus =
            await ValidateNodeID(newParentID);
        if(nodeIDValidationStatus.Failure()) {
            return nodeIDValidationStatus;
        }
        StatusMessage newParentIsInNodeSubtreeStatus =
            await FailIfNodeInSubtree(newParentID, nodeID);
        if(newParentIsInNodeSubtreeStatus.Failure()) {
            return newParentIsInNodeSubtreeStatus;
        }
        return await repo.MoveNodesSubtree(nodeID, newParentID);
    }

    public async Task<StatusMessage> SetNodeTags(
        int nodeID,
        IEnumerable<int> tagIDs
    ) {
        StatusMessage nodeIDValidationStatus = await ValidateNodeID(nodeID);
        if(nodeIDValidationStatus.Failure()) {
            return nodeIDValidationStatus;
        }
        StatusMessage tagsIDsValidationStatus = await ValidateTagsIDs(tagIDs);
        if(tagsIDsValidationStatus.Failure()) {
            return tagsIDsValidationStatus;
        }
        return await repo.SetNodeTags(nodeID, tagIDs);
    }
    

    public async Task<StatusMessage> UpdateNode(
        NtwkNode node
    ) {
        StatusMessage nodeIDValidationStatus = await ValidateNodeID(node.ID);
        if(nodeIDValidationStatus.Failure()) {
            return nodeIDValidationStatus;
        }
        StatusMessage nodeValidationStatus = validator.Validate(node);
        if(nodeValidationStatus.Failure()) {
            return nodeValidationStatus;
        }
        StatusMessage nameExistsStatus =
            await FailIfNodeNameExists(node.Name, updatingNodeID: node.ID);
        if(nameExistsStatus.Failure()) {
            return nameExistsStatus;
        }
        return await repo.UpdateNode(viewToEFConverter.Convert(node));
    }
    

    public async Task<DataActionResult<NtwkNode>> RemoveNode(int nodeID) {
        StatusMessage nodeIDValidationStatus = await ValidateNodeID(nodeID);
        if(nodeIDValidationStatus.Failure()) {
            return DataActionResult<NtwkNode>.Failed(nodeIDValidationStatus);
        }
        return FailOrConvert(
            await repo.RemoveNode(nodeID),
            n => EFToViewConverter.Convert(n)
        );
    }
}

}