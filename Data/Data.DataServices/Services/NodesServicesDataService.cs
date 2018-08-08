using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;

using Data.Abstract.DataAccessServices;
using Data.Abstract.DbInteraction;
using Data.Abstract.Validation;
using Data.Abstract.Converters;
using Data.Model.ViewModel;
using EFDbModel = Data.Model.EFDbModel;

namespace Data.DataServices.Services {

public class NodesServicesDataService : INodesServicesDataService {
    readonly IDataRepository repo;
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;
    readonly CommonServiceUtils utils;

    public NodesServicesDataService(
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

    public async Task<DataActionResult<IPAddress>> GetNodeIP(int nodeID) {
        string nValidationError = await utils.ValidateNodeID(nodeID);
        if(nValidationError != null) {
            return utils.FailActResult<IPAddress>(nValidationError);
        }
        DbOperationResult<uint> nodeIP =
            await repo.GetNodeIP(nodeID);
        if(!nodeIP.Success) {
            return utils.FailActResult<IPAddress>("Unable to get node IP address from database.");
        }
        return utils.SuccActResult(new IPAddress(nodeIP.Result));
    }

    public async Task<DataActionResult<string>> GetCWSBoundingString(int nodeID, int cwsID) {
        string nValidationError = await utils.ValidateNodeID(nodeID);
        if(nValidationError != null) {
            return utils.FailActResult<string>(nValidationError);
        }
        DbOperationResult<int> paramNum =
            await repo.GetCWSParamNumber(cwsID);
        if(!paramNum.Success) {
            return utils.FailActResult<string>("Unable to get CWS parameter number from database.");
        }
        else if(paramNum.Result == -1) {
            return utils.FailActResult<string>("Invalid CWS ID.");
        }
        DbOperationResult<string> serviceString =
            await repo.GetCWSBoundingString(nodeID, cwsID);
        if(!serviceString.Success) {
            return utils.FailActResult<string>(
                "Unable to get node web service string from database."
            );
        }
        return utils.SuccActResult<string>(
            serviceString.Result
        );
    }
}

}