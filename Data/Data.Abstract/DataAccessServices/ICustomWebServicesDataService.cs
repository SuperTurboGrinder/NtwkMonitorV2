using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through DataActionResult
public interface ICustomWebServicesDataService {
    Task<DataActionResult<IEnumerable<CustomWebService>>> GetAllCWS();

    Task<DataActionResult<CustomWebService>> CreateCustomWebService(CustomWebService cws);
    Task<StatusMessage> CreateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);

    Task<StatusMessage> UpdateCustomWebService(CustomWebService cws);
    Task<StatusMessage> UpdateWebServiceBinding(int nodeID, int cwsID,
        string param1, string param2, string param3);

    Task<DataActionResult<CustomWebService>> RemoveCustomWebService(int cwsID);
    Task<StatusMessage> RemoveWebServiceBinding(int nodeID, int cwsID);
}

}