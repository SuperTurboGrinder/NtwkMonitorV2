using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Data.Abstract.DataAccessServices;
using Data.Abstract.DbInteraction;
using Data.Abstract.Validation;
using Data.Abstract.Converters;
using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.DataServices.Services
{
    public class NodeTreeDataService
        : BaseDataService, INodeTreeDataService
    {
        readonly IViewModelValidator _validator;
        readonly IViewModelToEfModelConverter _viewToEfConverter;
        readonly IEfModelToViewModelConverter _efToViewConverter;

        public NodeTreeDataService(
            IDataRepository repo,
            IViewModelValidator validator,
            IViewModelToEfModelConverter viewToEfConverter,
            IEfModelToViewModelConverter efToViewConverter
        ) : base(repo)
        {
            _validator = validator;
            _viewToEfConverter = viewToEfConverter;
            _efToViewConverter = efToViewConverter;
        }

        public async Task<DataActionResult<AllNodesData>> GetAllNodesData()
        {
            return FailOrConvert(
                await Repo.GetAllNodesData(),
                nodesData => new AllNodesData()
                {
                    WebServicesData = nodesData.WebServicesData,
                    NodesData = nodesData.NodesData
                        .Select(ndGroup => ndGroup
                            .Select(nd => new NodeData()
                            {
                                Node = _efToViewConverter.Convert(nd.Node),
                                BoundWebServicesIDs = nd.BoundWebServicesIDs,
                                TagsIDs = nd.TagsIDs
                            })
                        )
                        .ToList()
                }
            );
        }

        public async Task<DataActionResult<IEnumerable<int>>> GetTaggedNodesIDs(
            int tagId
        )
        {
            StatusMessage tagIdValidationStatus = await ValidateTagId(tagId);
            if (tagIdValidationStatus.Failure())
            {
                return DataActionResult<IEnumerable<int>>.Failed(tagIdValidationStatus);
            }

            return await Repo.GetTaggedNodesIDs(tagId);
        }


        public async Task<DataActionResult<NtwkNode>> CreateNodeOnRoot(NtwkNode node)
        {
            StatusMessage nodeValidationStatus = _validator.Validate(node);
            if (nodeValidationStatus.Failure())
            {
                return DataActionResult<NtwkNode>.Failed(nodeValidationStatus);
            }

            StatusMessage nameExistsStatus = await FailIfNodeNameExists(node.Name);
            if (nameExistsStatus.Failure())
            {
                return DataActionResult<NtwkNode>.Failed(nameExistsStatus);
            }

            return FailOrConvert(
                await Repo.CreateNodeOnRoot(_viewToEfConverter.Convert(node)),
                n => _efToViewConverter.Convert(n)
            );
        }

        public async Task<DataActionResult<NtwkNode>> CreateNodeWithParent(
            NtwkNode node,
            int parentId
        )
        {
            StatusMessage nodeIdValidationStatus = await ValidateNodeId(parentId);
            if (nodeIdValidationStatus.Failure())
            {
                return DataActionResult<NtwkNode>.Failed(nodeIdValidationStatus);
            }

            StatusMessage nodeValidationStatus = _validator.Validate(node);
            if (nodeValidationStatus.Failure())
            {
                return DataActionResult<NtwkNode>.Failed(nodeValidationStatus);
            }

            StatusMessage nameExistsStatus = await FailIfNodeNameExists(node.Name);
            if (nameExistsStatus.Failure())
            {
                return DataActionResult<NtwkNode>.Failed(nameExistsStatus);
            }

            return FailOrConvert(
                await Repo.CreateNodeWithParent(_viewToEfConverter.Convert(node), parentId),
                n => _efToViewConverter.Convert(n)
            );
        }


        public async Task<StatusMessage> MoveNodesSubtree(
            int nodeId,
            int newParentId
        )
        {
            StatusMessage nodeIdValidationStatus = await ValidateNodeId(nodeId);
            if (nodeIdValidationStatus.Failure())
            {
                return nodeIdValidationStatus;
            }

            StatusMessage newParentIdValidationStatus = newParentId == 0
                ? StatusMessage.Ok //imaginary root id
                : await ValidateNodeId(newParentId);
            if (newParentIdValidationStatus.Failure())
            {
                return nodeIdValidationStatus;
            }

            StatusMessage newParentIsInNodeSubtreeStatus =
                await FailIfNodeInSubtree(newParentId, nodeId);
            if (newParentIsInNodeSubtreeStatus.Failure())
            {
                return newParentIsInNodeSubtreeStatus;
            }

            return await Repo.MoveNodesSubtree(nodeId, newParentId);
        }

        public async Task<StatusMessage> SetNodeTags(
            int nodeId,
            IEnumerable<int> tagIDs
        )
        {
            StatusMessage nodeIdValidationStatus = await ValidateNodeId(nodeId);
            if (nodeIdValidationStatus.Failure())
            {
                return nodeIdValidationStatus;
            }

            IEnumerable<int> tagsIDs = tagIDs as int[] ?? tagIDs.ToArray();
            StatusMessage tagsIDsValidationStatus = await ValidateTagsIDs(tagsIDs);
            if (tagsIDsValidationStatus.Failure())
            {
                return tagsIDsValidationStatus;
            }

            return await Repo.SetNodeTags(nodeId, tagsIDs);
        }


        public async Task<StatusMessage> UpdateNode(
            NtwkNode node
        )
        {
            StatusMessage nodeIdValidationStatus = await ValidateNodeId(node.Id);
            if (nodeIdValidationStatus.Failure())
            {
                return nodeIdValidationStatus;
            }

            StatusMessage nodeValidationStatus = _validator.Validate(node);
            if (nodeValidationStatus.Failure())
            {
                return nodeValidationStatus;
            }

            StatusMessage nameExistsStatus =
                await FailIfNodeNameExists(node.Name, updatingNodeId: node.Id);
            if (nameExistsStatus.Failure())
            {
                return nameExistsStatus;
            }

            return await Repo.UpdateNode(_viewToEfConverter.Convert(node));
        }


        public async Task<DataActionResult<NtwkNode>> RemoveNode(int nodeId)
        {
            StatusMessage nodeIdValidationStatus = await ValidateNodeId(nodeId);
            if (nodeIdValidationStatus.Failure())
            {
                return DataActionResult<NtwkNode>.Failed(nodeIdValidationStatus);
            }

            return FailOrConvert(
                await Repo.RemoveNode(nodeId),
                n => _efToViewConverter.Convert(n)
            );
        }
    }
}