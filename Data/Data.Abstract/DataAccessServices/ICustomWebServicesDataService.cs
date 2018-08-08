using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through DataActionResult
public interface ICustomWebServicesDataService {
    Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCWS();
    Task<DataActionResult<CWSBondExistanceMapping>> GetCWSBondExistanceMapping();

    Task<DataActionResult<CustomWebService>> CreateCustomWebService(CustomWebService cws);
    Task<DataActionVoidResult> CreateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);

    Task<DataActionVoidResult> UpdateCustomWebService(CustomWebService cws);
    Task<DataActionVoidResult> UpdateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);

    Task<DataActionResult<CustomWebService>> RemoveCustomWebService(int cwsID);
    Task<DataActionVoidResult> RemoveWebServiceBinding(int nodeID, int cwsID);
}

}