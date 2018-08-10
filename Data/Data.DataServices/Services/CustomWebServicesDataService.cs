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

public class CustomWebServicesDataService : ICustomWebServicesDataService {
    readonly IDataRepository repo;
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;
    readonly CommonServiceUtils utils;

    public CustomWebServicesDataService(
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

    public async Task<DataActionResult<CWSBondExistanceMapping>> GetCWSBondExistanceMapping() {
        DbOperationResult<CWSBondExistanceMapping> servicesMapping =
            await repo.GetCWSBondExistanceMapping();
        if(!servicesMapping.Success) {
            return utils.FailActResult<CWSBondExistanceMapping>(
                "Unable to get web service bindings mapping from database."
            );
        }
        return utils.SuccActResult<CWSBondExistanceMapping>(
            servicesMapping.Result
        );
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