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

public class TagsDataService : ITagsDataService {
    readonly IDataRepository repo;
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;
    readonly CommonServiceUtils utils;

    public TagsDataService(
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
}

}