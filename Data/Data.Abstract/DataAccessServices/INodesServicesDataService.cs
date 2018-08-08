using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Net;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//reporting errors through DataActionResult
public interface INodesServicesDataService {
    Task<DataActionResult<IPAddress>> GetNodeIP(int nodeID);
    Task<DataActionResult<string>> GetCWSBoundingString(int nodeID, int cwsID);
}

}