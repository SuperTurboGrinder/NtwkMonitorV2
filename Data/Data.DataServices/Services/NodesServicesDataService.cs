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
using Data.Model.ResultsModel;

namespace Data.DataServices.Services {

public class NodesServicesDataService
    : BaseDataService, INodesServicesDataService {
    readonly IViewModelValidator validator;
    readonly IViewModelToEFModelConverter viewToEFConverter;
    readonly IEFModelToViewModelConverter EFToViewConverter;

    public NodesServicesDataService(
        IDataRepository _repo,
        IViewModelValidator _validator,
        IViewModelToEFModelConverter _viewToEFConverter,
        IEFModelToViewModelConverter _EFToViewConverter
    ) : base(_repo) {
        validator = _validator;
        viewToEFConverter = _viewToEFConverter;
        EFToViewConverter = _EFToViewConverter;
    }

    public async Task<DataActionResult<IPAddress>> GetNodeIP(int nodeID) {
        StatusMessage nodeIDValidationStatus = await ValidateNodeID(nodeID);
        if(nodeIDValidationStatus.Failure()) {
            return DataActionResult<IPAddress>.Failed(nodeIDValidationStatus);
        }
        return FailOrConvert(
            await repo.GetNodeIP(nodeID),
            nIP => new IPAddress(nIP)
        );
    }

    public async Task<DataActionResult<string>> GetCWSBoundingString(
        int nodeID,
        int cwsID
    ) {
        StatusMessage nodeIDValidationStatus = await ValidateNodeID(nodeID);
        if(nodeIDValidationStatus.Failure()) {
            return DataActionResult<string>.Failed(nodeIDValidationStatus);
        }
        StatusMessage cwsIDValidationStatus =
            (await GetCWSParamNumber(cwsID)).Status;
        if(cwsIDValidationStatus.Failure()) {
            return DataActionResult<string>.Failed(cwsIDValidationStatus);
        }
        StatusMessage cwsBindingExistsStatus =
            await FailIfCWSBinding_DOES_NOT_Exist(nodeID, cwsID);
        if(cwsBindingExistsStatus.Failure()) {
            return DataActionResult<string>.Failed(cwsBindingExistsStatus);
        }
        return await repo.GetCWSBoundingString(nodeID, cwsID);
    }
}

}