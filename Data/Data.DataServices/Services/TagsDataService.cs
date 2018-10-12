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

public class TagsDataService
    : BaseDataService, ITagsDataService {
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;

    public TagsDataService(
        IDataRepository _repo,
        IViewModelValidator _validator,
        IViewModelToEFModelConverter _viewToEFConverter,
        IEFModelToViewModelConverter _EFToViewConverter
    ) : base(_repo) {
        validator = _validator;
        viewToEFConverter = _viewToEFConverter;
        EFToViewConverter = _EFToViewConverter;
    }

    
    public async Task<DataActionResult<IEnumerable<NodeTag>>> GetAllTags() {
        return FailOrConvert(
            await repo.GetAllTags(),
            tags => tags.Select(t => EFToViewConverter.Convert(t))
        );
    }
    
    public async Task<DataActionResult<NodeTag>> CreateTag(NodeTag tag) {
        StatusMessage tagValidationStatus = validator.Validate(tag);
        if(tagValidationStatus.Failure()) {
            return DataActionResult<NodeTag>.Failed(tagValidationStatus);
        }
        StatusMessage nameExistsStatus = await FailIfTagNameExists(tag.Name);
        if(nameExistsStatus.Failure()) {
            return DataActionResult<NodeTag>.Failed(nameExistsStatus);
        }
        return FailOrConvert(
            await repo.CreateTag(viewToEFConverter.Convert(tag)),
            t => EFToViewConverter.Convert(t)
        );
    }

    public async Task<StatusMessage> UpdateTag(NodeTag tag) {
        StatusMessage tagIDValidationStatus = await ValidateTagID(tag.ID);
        if(tagIDValidationStatus.Failure()) {
            return tagIDValidationStatus;
        }
        StatusMessage tagValidationStatus = validator.Validate(tag);
        if(tagValidationStatus.Failure()) {
            return tagValidationStatus;
        }
        return await repo.UpdateTag(viewToEFConverter.Convert(tag));
    }

    public async Task<DataActionResult<NodeTag>> RemoveTag(int tagID) {
        StatusMessage tagIDValidationStatus = await ValidateTagID(tagID);
        if(tagIDValidationStatus.Failure()) {
            return DataActionResult<NodeTag>.Failed(tagIDValidationStatus);
        }
        return FailOrConvert(
            await repo.RemoveTag(tagID),
            t => EFToViewConverter.Convert(t)
        );
    }
}

}