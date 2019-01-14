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
    public class CustomWebServicesDataService
        : BaseDataService, ICustomWebServicesDataService
    {
        readonly IViewModelValidator _validator;
        readonly IViewModelToEfModelConverter _viewToEfConverter;
        readonly IEfModelToViewModelConverter _efToViewConverter;

        public CustomWebServicesDataService(
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

        public async Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCws()
        {
            return FailOrConvert(
                await Repo.GetAllCws(),
                enM => enM.Select(m => _efToViewConverter.Convert(m))
            );
        }

        async Task<StatusMessage> CwsBindingValidationRoutine(
            int nodeId,
            int cwsId,
            string param1,
            string param2,
            string param3
        )
        {
            StatusMessage idValidationStatus = await ValidateNodeId(nodeId);
            if (idValidationStatus.Failure())
            {
                return idValidationStatus;
            }

            DataActionResult<int> paramNumResult =
                await GetCwsParamNumber(cwsId);
            if (paramNumResult.Status.Failure())
            {
                return paramNumResult.Status;
            }

            int n = paramNumResult.Result;
            return (
                (n != 0 && param1 == null) ||
                (n > 1 && param2 == null) ||
                (n == 3 && param3 == null)
            )
                ? StatusMessage.ServiceBindingSetParementersValueCanNotBeNull
                : (
                    (n < 3 && param3 != null) ||
                    (n < 2 && param2 != null) ||
                    (n == 0 && param1 != null)
                )
                    ? StatusMessage.RedundantParameterValuesInServiceBinding
                    : StatusMessage.Ok;
        }

        public async Task<StatusMessage> CreateWebServiceBinding(
            int nodeId,
            int cwsId,
            string param1,
            string param2,
            string param3
        )
        {
            StatusMessage wsbValidationStatus = await CwsBindingValidationRoutine(
                nodeId, cwsId, param1, param2, param3
            );
            if (wsbValidationStatus.Failure())
            {
                return wsbValidationStatus;
            }

            StatusMessage newUniqBindingStatus = await FailIfCwsBindingExists(cwsId, nodeId);
            if (newUniqBindingStatus.Failure())
            {
                return newUniqBindingStatus;
            }

            return await Repo.CreateWebServiceBinding(nodeId, cwsId, param1, param2, param3);
        }

        public async Task<DataActionResult<CustomWebService>> CreateCustomWebService(
            CustomWebService cws
        )
        {
            StatusMessage validationStatus = _validator.Validate(cws);
            if (validationStatus.Failure())
            {
                return DataActionResult<CustomWebService>.Failed(validationStatus);
            }

            StatusMessage nameExistsStatus =
                await FailIfCwsNameExists(cws.Name);
            if (nameExistsStatus.Failure())
            {
                return DataActionResult<CustomWebService>.Failed(nameExistsStatus);
            }

            return FailOrConvert(
                await Repo.CreateCustomWebService(_viewToEfConverter.Convert(cws)),
                created => _efToViewConverter.Convert(created)
            );
        }


        public async Task<StatusMessage> UpdateCustomWebService(
            CustomWebService cws
        )
        {
            StatusMessage idExistsStatus =
                (await GetCwsParamNumber(cws.Id)).Status;
            if (idExistsStatus.Failure())
            {
                return idExistsStatus;
            }

            StatusMessage validationStatus = _validator.Validate(cws);
            if (validationStatus.Failure())
            {
                return validationStatus;
            }

            StatusMessage nameExistsStatus =
                await FailIfCwsNameExists(cws.Name, cws.Id);
            if (nameExistsStatus.Failure())
            {
                return nameExistsStatus;
            }

            return await Repo.UpdateCustomWebService(_viewToEfConverter.Convert(cws));
        }

        public async Task<StatusMessage> UpdateWebServiceBinding(
            int nodeId,
            int cwsId,
            string param1,
            string param2,
            string param3
        )
        {
            StatusMessage validationStatus = await CwsBindingValidationRoutine(
                nodeId, cwsId, param1, param2, param3
            );
            if (validationStatus.Failure())
            {
                return validationStatus;
            }

            return await Repo.UpdateWebServiceBinding(
                nodeId, cwsId, param1, param2, param3
            );
        }


        public async Task<DataActionResult<CustomWebService>> RemoveCustomWebService(
            int cwsId
        )
        {
            StatusMessage idExistsStatus =
                (await GetCwsParamNumber(cwsId)).Status;
            if (idExistsStatus.Failure())
            {
                return DataActionResult<CustomWebService>.Failed(idExistsStatus);
            }

            return FailOrConvert(
                await Repo.RemoveCustomWebService(cwsId),
                cws => _efToViewConverter.Convert(cws)
            );
        }

        public async Task<StatusMessage> RemoveWebServiceBinding(
            int nodeId,
            int cwsId
        )
        {
            StatusMessage nodeIdValidationStatus = await ValidateNodeId(nodeId);
            if (nodeIdValidationStatus.Failure())
            {
                return nodeIdValidationStatus;
            }

            StatusMessage cwsIdValidationStatus =
                (await GetCwsParamNumber(cwsId)).Status;
            if (cwsIdValidationStatus.Failure())
            {
                return cwsIdValidationStatus;
            }

            return await Repo.RemoveWebServiceBinding(nodeId, cwsId);
        }
    }
}