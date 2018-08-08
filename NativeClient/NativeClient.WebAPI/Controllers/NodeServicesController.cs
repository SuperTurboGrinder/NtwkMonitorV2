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
    readonly INodesServicesDataService data;
    readonly IPingService pingService;
    readonly IExecutablesManagerService execServices;
    readonly IWebServiceLauncherService webServices;

    NodeServicesController(
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
    
    //GET api/services/customWebService/1/1
    [HttpGet("/customWebService/{nodeID:int}/{cwsID:int}")]
    public async Task<ActionResult> OpenTelnetService(int nodeID, int cwsID) {
        DataActionResult<string> webServiceString =
            await data.GetCWSBoundingString(nodeID, cwsID);
        if(!webServiceString.Success) {
            return BadRequest(webServiceString.Error);
        }
        if(webServiceString.Result == null) {
            return BadRequest("Web service binding not found.");
        }
        string serviceStartError =
            webServices.Start(webServiceString.Result);
        if(serviceStartError != null) {
            return BadRequest(serviceStartError);
        }
        return Ok();
    }
}

}