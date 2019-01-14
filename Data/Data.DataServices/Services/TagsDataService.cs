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
    public class TagsDataService
        : BaseDataService, ITagsDataService
    {
        readonly IViewModelValidator _validator;
        readonly IViewModelToEfModelConverter _viewToEfConverter;
        readonly IEfModelToViewModelConverter _efToViewConverter;

        public TagsDataService(
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


        public async Task<DataActionResult<IEnumerable<NodeTag>>> GetAllTags()
        {
            return FailOrConvert(
                await Repo.GetAllTags(),
                tags => tags.Select(t => _efToViewConverter.Convert(t))
            );
        }

        public async Task<DataActionResult<NodeTag>> CreateTag(NodeTag tag)
        {
            StatusMessage tagValidationStatus = _validator.Validate(tag);
            if (tagValidationStatus.Failure())
            {
                return DataActionResult<NodeTag>.Failed(tagValidationStatus);
            }

            StatusMessage nameExistsStatus = await FailIfTagNameExists(tag.Name);
            if (nameExistsStatus.Failure())
            {
                return DataActionResult<NodeTag>.Failed(nameExistsStatus);
            }

            return FailOrConvert(
                await Repo.CreateTag(_viewToEfConverter.Convert(tag)),
                t => _efToViewConverter.Convert(t)
            );
        }

        public async Task<StatusMessage> UpdateTag(NodeTag tag)
        {
            StatusMessage tagIdValidationStatus = await ValidateTagId(tag.Id);
            if (tagIdValidationStatus.Failure())
            {
                return tagIdValidationStatus;
            }

            StatusMessage tagValidationStatus = _validator.Validate(tag);
            if (tagValidationStatus.Failure())
            {
                return tagValidationStatus;
            }

            StatusMessage nameExistsStatus =
                await FailIfTagNameExists(tag.Name, updatingTagId: tag.Id);
            if (nameExistsStatus.Failure())
            {
                return nameExistsStatus;
            }

            return await Repo.UpdateTag(_viewToEfConverter.Convert(tag));
        }

        public async Task<DataActionResult<NodeTag>> RemoveTag(int tagId)
        {
            StatusMessage tagIdValidationStatus = await ValidateTagId(tagId);
            if (tagIdValidationStatus.Failure())
            {
                return DataActionResult<NodeTag>.Failed(tagIdValidationStatus);
            }

            return FailOrConvert(
                await Repo.RemoveTag(tagId),
                t => _efToViewConverter.Convert(t)
            );
        }
    }
}