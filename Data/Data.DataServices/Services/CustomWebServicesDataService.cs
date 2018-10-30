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

public class CustomWebServicesDataService
    : BaseDataService, ICustomWebServicesDataService {
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;

    public CustomWebServicesDataService(
        IDataRepository _repo,
        IViewModelValidator _validator,
        IViewModelToEFModelConverter _viewToEFConverter,
        IEFModelToViewModelConverter _EFToViewConverter
    ) : base(_repo) {
        validator = _validator;
        viewToEFConverter = _viewToEFConverter;
        EFToViewConverter = _EFToViewConverter;
    }
    
    public async Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCWS() {
        return FailOrConvert(
            await repo.GetAllCWS(),
            EnM => EnM.Select(m => EFToViewConverter.Convert(m))
        );
    }

    async Task<StatusMessage> CWSBindingValidationRoutine(
        int nodeID,
        int cwsID,
        string param1,
        string param2,
        string param3
    ) {
        StatusMessage idValidationStatus = await ValidateNodeID(nodeID);
        if(idValidationStatus.Failure()) {
            return idValidationStatus;
        }
        DataActionResult<int> paramNumResult =
            await GetCWSParamNumber(cwsID);
        if(paramNumResult.Status.Failure()) {
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
        int nodeID,
        int cwsID,
        string param1,
        string param2,
        string param3
    ) {
        StatusMessage wsbValidationStatus = await CWSBindingValidationRoutine(
            nodeID, cwsID, param1, param2, param3
        );
        if(wsbValidationStatus.Failure()) {
            return wsbValidationStatus;
        }
        StatusMessage newUniqBindingStatus = await FailIfCWSBindingExists(cwsID, nodeID);
        if(newUniqBindingStatus.Failure()) {
            return newUniqBindingStatus;
        }
        return await repo.CreateWebServiceBinding(nodeID, cwsID, param1, param2, param3);
    }
    
    public async Task<DataActionResult<CustomWebService>> CreateCustomWebService(
        CustomWebService cws
    ) {
        StatusMessage validationStatus = validator.Validate(cws);
        if(validationStatus.Failure()) {
            return DataActionResult<CustomWebService>.Failed(validationStatus);
        }
        StatusMessage nameExistsStatus =
            await FailIfCWSNameExists(cws.Name);
        if(nameExistsStatus.Failure()) {
            return DataActionResult<CustomWebService>.Failed(nameExistsStatus);
        }
        return FailOrConvert(
            await repo.CreateCustomWebService(viewToEFConverter.Convert(cws)),
            created => EFToViewConverter.Convert(created)
        );
    }
    
    
    public async Task<StatusMessage> UpdateCustomWebService(
        CustomWebService cws
    ) {
        StatusMessage idExistsStatus =
            (await GetCWSParamNumber(cws.ID)).Status;
        if(idExistsStatus.Failure()) {
            return idExistsStatus;
        }
        StatusMessage validationStatus = validator.Validate(cws);
        if(validationStatus.Failure()) {
            return validationStatus;
        }
        StatusMessage nameExistsStatus =
            await FailIfCWSNameExists(cws.Name, updatingCWSID: cws.ID);
        if(nameExistsStatus.Failure()) {
            return nameExistsStatus;
        }
        return await repo.UpdateCustomWebService(viewToEFConverter.Convert(cws));
    }
    
    public async Task<StatusMessage> UpdateWebServiceBinding(
        int nodeID,
        int cwsID,
        string param1,
        string param2,
        string param3
    ) {
        StatusMessage validationStatus = await CWSBindingValidationRoutine(
            nodeID, cwsID, param1, param2, param3
        );
        if(validationStatus.Failure()) {
            return validationStatus;
        }
        return await repo.UpdateWebServiceBinding(
            nodeID, cwsID, param1, param2, param3
        );
    }


    public async Task<DataActionResult<CustomWebService>> RemoveCustomWebService(
        int cwsID
    ) {
        StatusMessage idExistsStatus =
            (await GetCWSParamNumber(cwsID)).Status;
        if(idExistsStatus.Failure()) {
            return DataActionResult<CustomWebService>.Failed(idExistsStatus);
        }
        return FailOrConvert(
            await repo.RemoveCustomWebService(cwsID),
            cws => EFToViewConverter.Convert(cws)
        );
    }
    
    public async Task<StatusMessage> RemoveWebServiceBinding(
        int nodeID,
        int cwsID
    ) {
        StatusMessage nodeIDValidationStatus = await ValidateNodeID(nodeID);
        if(nodeIDValidationStatus.Failure()) {
            return nodeIDValidationStatus;
        }
        StatusMessage cwsIDValidationStatus =
            (await GetCWSParamNumber(cwsID)).Status;
        if(cwsIDValidationStatus.Failure()) {
            return cwsIDValidationStatus;
        }
        return await repo.RemoveWebServiceBinding(nodeID, cwsID);
    }
}

}