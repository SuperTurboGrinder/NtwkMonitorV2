using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Controllers {

[Route("api/services")]
public class NodeServicesController : Controller {
    readonly INodesIPsDataService data;
    readonly IPingService pingService;

    NodeServicesController(INodesIPsDataService _data, IPingService _pingService) {
        data = _data;
        pingService = _pingService;
    }
    
    // GET api/services/ping/1
    [HttpGet("/ping/{nodeID:int}")]
    public async Task<ActionResult> GetNodePing(int nodeID) {
        DataActionResult<uint> rawNodeIP = await data.GetNodeIP(nodeID);
        if(!rawNodeIP.Success) {
            return BadRequest(rawNodeIP.Error);
        }
        IPAddress ip = new IPAddress(rawNodeIP.Result);
        Services.Model.PingTestData pingResult =
            await pingService.TestConnectionAsync(ip);
        if(
            pingResult.error != null &&
            (pingResult.num - pingResult.failed) == 0
        ) {
            return BadRequest(pingResult.error);
        }
        return Ok(pingResult);
    }
}

}