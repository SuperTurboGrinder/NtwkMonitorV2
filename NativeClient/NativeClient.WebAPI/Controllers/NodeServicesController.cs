using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;
using NativeClient.WebAPI.Abstract;
using NativeClient.WebAPI.Services.Model;

namespace NativeClient.WebAPI.Controllers {

[Route("api/services")]
public class NodeServicesController : Controller {
    readonly INodesIPsDataService data;
    readonly IPingService pingService;
    readonly IExecutablesManagerService execServices;

    NodeServicesController(
        INodesIPsDataService _data,
        IPingService _pingService,
        IExecutablesManagerService _execServices
    ) {
        data = _data;
        pingService = _pingService;
        execServices = _execServices;
    }

    async Task<ActionResult> OperationWithIP(
        int nodeID,
        Func<IPAddress, Task<ActionResult>> operation
    ) {
        DataActionResult<IPAddress> rawNodeIP = await data.GetNodeIP(nodeID);
        if(!rawNodeIP.Success) {
            return BadRequest(rawNodeIP.Error);
        }
        return await operation(rawNodeIP.Result);
    }

    async Task<ActionResult> OpenExecutableService(
        int nodeID,
        ExecutableServicesTypes serviceType
    ) {
        return await OperationWithIP(nodeID, async (IPAddress ip) => {
            string serviceExecutionError =
                execServices.ExecuteService(serviceType, ip);
            if(serviceExecutionError != null) {
                return BadRequest(serviceExecutionError);
            }
            return Ok();
        });
    }
    
    // GET api/services/ping/1
    [HttpGet("/ping/{nodeID:int}")]
    public async Task<ActionResult> GetNodePing(int nodeID) {
        return await OperationWithIP(nodeID, async (IPAddress ip) => {
            Services.Model.PingTestData pingResult =
                await pingService.TestConnectionAsync(ip);
            if(
                pingResult.error != null &&
                pingResult.num == pingResult.failed
            ) {
                return BadRequest(pingResult.error);
            }
            return Ok(pingResult);
        });
    }

    //GET api/services/ssh/1
    [HttpGet("/ssh/{nodeID:int}")]
    public async Task<ActionResult> OpenSSHService(int nodeID) {
        return await OpenExecutableService(nodeID, ExecutableServicesTypes.SSH);
    }

    //GET api/services/telnet/1
    [HttpGet("/telnet/{nodeID:int}")]
    public async Task<ActionResult> OpenTelnetService(int nodeID) {
        return await OpenExecutableService(nodeID, ExecutableServicesTypes.Telnet);
    }
}

}