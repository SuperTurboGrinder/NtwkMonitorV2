using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Data.Abstract.DataAccessServices;
using Data.Model.ResultsModel;
using Microsoft.AspNetCore.Mvc;
using NativeClient.WebAPI.Abstract;
using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Controllers
{
    [Route("api/services")]
    public class NodeServicesController : BaseDataController
    {
        readonly INodesServicesDataService _data;
        readonly IExecutablesManagerService _execServices;
        readonly IPingService _pingService;
        readonly IWebServiceLauncherService _webServices;

        public NodeServicesController(
            INodesServicesDataService data,
            IPingService pingService,
            IExecutablesManagerService execServices,
            IWebServiceLauncherService webServices,
            IErrorReportAssemblerService errAssembler
        ) : base(errAssembler)
        {
            _data = data;
            _pingService = pingService;
            _execServices = execServices;
            _webServices = webServices;
        }

        async Task<ActionResult> ObserveAsyncDataOperationResultWithIp<T>(
            int nodeId,
            Func<IPAddress, Task<DataActionResult<T>>> operation
        )
        {
            DataActionResult<IPAddress> nodeIpResult = await _data.GetNodeIp(nodeId);
            return nodeIpResult.Status.Failure()
                ? BadRequest(nodeIpResult.Status)
                : ObserveDataOperationResult(
                    await operation(nodeIpResult.Result)
                );
        }

        async Task<ActionResult> ObserveAsyncDataOperationResultWithIpList<T>(
            IEnumerable<int> nodesIDs,
            Func<IEnumerable<IPAddress>, Task<DataActionResult<T>>> operation
        )
        {
            DataActionResult<IEnumerable<IPAddress>> nodesIPsResult =
                await _data.GetNodesIPs(nodesIDs);
            return nodesIPsResult.Status.Failure()
                ? BadRequest(nodesIPsResult.Status)
                : ObserveDataOperationResult(
                    await operation(nodesIPsResult.Result)
                );
        }

        async Task<ActionResult> ObserveDataOperationStatusWithIp(
            int nodeId,
            Func<IPAddress, StatusMessage> operation
        )
        {
            DataActionResult<IPAddress> nodeIpResult = await _data.GetNodeIp(nodeId);
            return nodeIpResult.Status.Failure()
                ? BadRequest(nodeIpResult.Status)
                : ObserveDataOperationStatus(
                    operation(nodeIpResult.Result)
                );
        }

        async Task<ActionResult> OpenExecutableService(
            int nodeId,
            ExecutableServicesTypes serviceType
        )
        {
            return await ObserveDataOperationStatusWithIp(
                nodeId,
                ip => _execServices.ExecuteService(serviceType, ip)
            );
        }

        // GET api/services/ping/1
        [HttpGet("ping/{nodeID:int}")]
        public async Task<ActionResult> GetNodePing(int nodeId)
        {
            return await ObserveAsyncDataOperationResultWithIp(
                nodeId,
                async ip => await _pingService.TestConnectionAsync(ip)
            );
        }

        // POST api/services/pingList
        [HttpPost("pingList")]
        public async Task<ActionResult> GetNodesListPing(
            [FromBody] int[] nodesIDs
        )
        {
            return await ObserveAsyncDataOperationResultWithIpList(
                nodesIDs,
                async ips => await _pingService.TestConnectionListAsync(ips)
            );
        }

        //GET api/services/ssh/1
        [HttpGet("ssh/{nodeID:int}")]
        public async Task<ActionResult> OpenSshService(int nodeId)
        {
            return await OpenExecutableService(nodeId, ExecutableServicesTypes.Ssh);
        }

        //GET api/services/telnet/1
        [HttpGet("telnet/{nodeID:int}")]
        public async Task<ActionResult> OpenTelnetService(int nodeId)
        {
            return await OpenExecutableService(nodeId, ExecutableServicesTypes.Telnet);
        }

        //GET api/services/customWebService/1/1
        [HttpGet("customWebService/{nodeID:int}/{cwsID:int}")]
        public async Task<ActionResult> OpenWebService(int nodeId, int cwsId)
        {
            DataActionResult<string> webServiceStringResult =
                await _data.GetCwsBoundingString(nodeId, cwsId);
            if (webServiceStringResult.Status.Failure())
                return ObserveDataOperationStatus(webServiceStringResult.Status);
            return ObserveDataOperationStatus(
                _webServices.Start(webServiceStringResult.Result)
            );
        }

        //GET api/services/customWebServiceString/1/1
        [HttpGet("customWebServiceString/{nodeID:int}/{cwsID:int}")]
        public async Task<ActionResult> GetWebServiceString(int nodeId, int cwsId)
        {
            return ObserveDataOperationResult(
                await _data.GetCwsBoundingString(nodeId, cwsId)
            );
        }
    }
}