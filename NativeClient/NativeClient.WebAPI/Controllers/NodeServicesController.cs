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
public class NodeServicesController : Controller {
    readonly INodesServicesDataService data;
    readonly IPingService pingService;
    readonly IExecutablesManagerService execServices;
    readonly IWebServiceLauncherService webServices;

    public NodeServicesController(
        INodesServicesDataService _data,
        IPingService _pingService,
        IExecutablesManagerService _execServices,
        IWebServiceLauncherService _webServices
    ) {
        data = _data;
        pingService = _pingService;
        execServices = _execServices;
        webServices = _webServices;
    }

    async Task<ActionResult> OperationWithIP(
        int nodeID,
        Func<IPAddress, Task<ActionResult>> operation
    ) {
        DataActionResult<IPAddress> nodeIPResult = await data.GetNodeIP(nodeID);
        return nodeIPResult.Status.Failure()
            ? BadRequest(nodeIPResult.Status)
            : await operation(nodeIPResult.Result);
    }

    async Task<ActionResult> SyncOperationWithIP(
        int nodeID,
        Func<IPAddress, ActionResult> operation
    ) {
        DataActionResult<IPAddress> nodeIPResult = await data.GetNodeIP(nodeID);
        return nodeIPResult.Status.Failure()
            ? BadRequest(nodeIPResult.Status)
            : operation(nodeIPResult.Result);
    }

    async Task<ActionResult> OpenExecutableService(
        int nodeID,
        ExecutableServicesTypes serviceType
    ) {
        return await SyncOperationWithIP(nodeID, (IPAddress ip) => {
            StatusMessage serviceExecutionStatus =
                execServices.ExecuteService(serviceType, ip);
            if(serviceExecutionStatus.Failure()) {
                return BadRequest(serviceExecutionStatus);
            }
            return Ok();
        });
    }
    
    // GET api/services/ping/1
    [HttpGet("ping/{nodeID:int}")]
    public async Task<ActionResult> GetNodePing(int nodeID) {
        return await OperationWithIP(nodeID, async (IPAddress ip) => {
            DataActionResult<Services.Model.PingTestData> pingResult =
                await pingService.TestConnectionAsync(ip);
            if(pingResult.Status.Failure()) {
                return BadRequest(pingResult.Status);
            }
            return Ok(pingResult.Result);
        });
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
            return BadRequest(webServiceStringResult.Status);
        }
        StatusMessage serviceStartStatus =
            webServices.Start(webServiceStringResult.Result);
        if(serviceStartStatus.Failure()) {
            return BadRequest(serviceStartStatus);
        }
        return Ok();
    }
}

}