using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using Data.Abstract.DataAccessServices;
using Data.Abstract.DbInteraction;
using Data.Model.ResultsModel;

namespace Data.DataServices.Services
{
    public class NodesServicesDataService
        : BaseDataService, INodesServicesDataService
    {
        public NodesServicesDataService(
            IDataRepository repo
        ) : base(repo)
        {
        }

        public async Task<DataActionResult<IPAddress>> GetNodeIp(int nodeId)
        {
            StatusMessage nodeIdValidationStatus = await ValidateNodeId(nodeId);
            if (nodeIdValidationStatus.Failure())
            {
                return DataActionResult<IPAddress>.Failed(nodeIdValidationStatus);
            }

            return FailOrConvert(
                await Repo.GetNodeIp(nodeId),
                nIp => new IPAddress(nIp)
            );
        }

        public async Task<DataActionResult<IEnumerable<IPAddress>>> GetNodesIPs(
            IEnumerable<int> nodesIDs
        )
        {
            var results = await Task.WhenAll(nodesIDs.Select(GetNodeIp));
            var firstFailure = results.FirstOrDefault(r => r.Status.Failure());
            return firstFailure != null
                ? DataActionResult<IEnumerable<IPAddress>>.Failed(firstFailure.Status)
                : DataActionResult<IEnumerable<IPAddress>>.Successful(results.Select(r => r.Result));
        }

        public async Task<DataActionResult<string>> GetCwsBoundingString(
            int nodeId,
            int cwsId
        )
        {
            StatusMessage nodeIdValidationStatus = await ValidateNodeId(nodeId);
            if (nodeIdValidationStatus.Failure())
            {
                return DataActionResult<string>.Failed(nodeIdValidationStatus);
            }

            StatusMessage cwsIdValidationStatus =
                (await GetCwsParamNumber(cwsId)).Status;
            if (cwsIdValidationStatus.Failure())
            {
                return DataActionResult<string>.Failed(cwsIdValidationStatus);
            }

            StatusMessage cwsBindingExistsStatus =
                await FailIfCWSBinding_DOES_NOT_Exist(nodeId, cwsId);
            if (cwsBindingExistsStatus.Failure())
            {
                return DataActionResult<string>.Failed(cwsBindingExistsStatus);
            }

            return await Repo.GetCwsBoundingString(nodeId, cwsId);
        }
    }
}