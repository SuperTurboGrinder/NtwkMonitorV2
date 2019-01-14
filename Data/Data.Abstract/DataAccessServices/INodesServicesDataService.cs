using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Data.Model.ResultsModel;

namespace Data.Abstract.DataAccessServices
{
//validation -> DataActionResult
    public interface INodesServicesDataService
    {
        Task<DataActionResult<IPAddress>> GetNodeIp(int nodeId);

        Task<DataActionResult<IEnumerable<IPAddress>>> GetNodesIPs(
            IEnumerable<int> nodesIDs
        );

        Task<DataActionResult<string>> GetCwsBoundingString(int nodeId, int cwsId);
    }
}