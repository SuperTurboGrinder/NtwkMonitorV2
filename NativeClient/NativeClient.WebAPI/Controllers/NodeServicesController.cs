using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;
using Data.Model.ResultsModel;
using NativeClient.WebAPI.Abstract;
using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Controllers {

[Route("api/services")]
public class NodeServicesController : BaseDataController {
    readonly INodesServicesDataService data;
    readonly IPingService pingService;
    readonly IExecutablesManagerService execServices;
    readonly IWebServiceLauncherService webServices;

    public NodeServicesController(
        INodesServicesDataService _data,
        IPingService _pingService,
        IExecutablesManagerService _execServices,
        IWebServiceLauncherService _webServices,
        IErrorReportAssemblerService _errAssembler
    ) : base(_errAssembler) {
        data = _data;
        pingService = _pingService;
        execServices = _execServices;
        webServices = _webServices;
    }

    async Task<ActionResult> ObserveAsyncDataOperationResultWithIP<T>(
        int nodeID,
        Func<IPAddress, Task<DataActionResult<T>>> operation
    ) {
        DataActionResult<IPAddress> nodeIPResult = await data.GetNodeIP(nodeID);
        return nodeIPResult.Status.Failure()
            ? BadRequest(nodeIPResult.Status)
            : ObserveDataOperationResult(
                await operation(nodeIPResult.Result)
            );
    }

    async Task<ActionResult> ObserveAsyncDataOperationResultWithIPList<T>(
        IEnumerable<int> nodesIDs,
        Func<IEnumerable<IPAddress>, Task<DataActionResult<T>>> operation
    ) {
        DataActionResult<IEnumerable<IPAddress>> nodesIPsResult =
            await data.GetNodesIPs(nodesIDs);
        return nodesIPsResult.Status.Failure()
            ? BadRequest(nodesIPsResult.Status)
            : ObserveDataOperationResult(
                await operation(nodesIPsResult.Result)
            );
    }

    async Task<ActionResult> ObserveDataOperationStatusWithIP(
        int nodeID,
        Func<IPAddress, StatusMessage> operation
    ) {
        DataActionResult<IPAddress> nodeIPResult = await data.GetNodeIP(nodeID);
        return nodeIPResult.Status.Failure()
            ? BadRequest(nodeIPResult.Status)
            : ObserveDataOperationStatus(
                operation(nodeIPResult.Result)
            );
    }

    async Task<ActionResult> OpenExecutableService(
        int nodeID,
        ExecutableServicesTypes serviceType
    ) {
        return await ObserveDataOperationStatusWithIP(
            nodeID,
            (IPAddress ip) => execServices.ExecuteService(serviceType, ip)
        );
    }
    
    // GET api/services/ping/1
    [HttpGet("ping/{nodeID:int}")]
    public async Task<ActionResult> GetNodePing(int nodeID) {
        return await ObserveAsyncDataOperationResultWithIP(
            nodeID,
            async (IPAddress ip) => await pingService.TestConnectionAsync(ip)
        );
    }

    // GET api/services/pingList/1
    [HttpGet("pingList")]
    public async Task<ActionResult> GetNodesListPing(
        [FromBody] IEnumerable<int> nodesIDs
    ) {
        return await ObserveAsyncDataOperationResultWithIPList(
            nodesIDs,
            async (IEnumerable<IPAddress> ips) =>
                await pingService.TestConnectionListAsync(ips)
        );
    }

    //GET api/services/ssh/1
    [HttpGet("ssh/{nodeID:int}")]
    public async Task<ActionResult> OpenSSHService(int nodeID) {
        return await OpenExecutableService(nodeID, ExecutableServicesTypes.SSH);
    }

    //GET api/services/telnet/1
    [HttpGet("telnet/{nodeID:int}")]
    public async Task<ActionResult> OpenTelnetService(int nodeID) {
        return await OpenExecutableService(nodeID, ExecutableServicesTypes.Telnet);
    }
    
    //GET api/services/customWebService/1/1
    [HttpGet("customWebService/{nodeID:int}/{cwsID:int}")]
    public async Task<ActionResult> OpenWebService(int nodeID, int cwsID) {
        DataActionResult<string> webServiceStringResult =
            await data.GetCWSBoundingString(nodeID, cwsID);
        if(webServiceStringResult.Status.Failure()) {
            return ObserveDataOperationStatus(webServiceStringResult.Status);
        }
        return ObserveDataOperationStatus(
            webServices.Start(webServiceStringResult.Result)
        );
    }

    //GET api/services/customWebServiceString/1/1
    [HttpGet("customWebServiceString/{nodeID:int}/{cwsID:int}")]
    public async Task<ActionResult> GetWebServiceString(int nodeID, int cwsID) {
        return ObserveDataOperationResult(
            await data.GetCWSBoundingString(nodeID, cwsID)
        );
    }
}

}