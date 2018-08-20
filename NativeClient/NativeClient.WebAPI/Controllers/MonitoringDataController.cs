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

[Route("api/monitorSessions")]
public class MonitoringDataController : BaseDataController {
    readonly IMonitoringDataService data;

    public MonitoringDataController(
        IMonitoringDataService _data,
        IErrorReportAssemblerService _errAssembler
    ) : base(_errAssembler) {
        data = _data;
    }

    // GET api/monitorSessions/forProfile/1
    [HttpGet("forProfile/{profileID:int}")]
    public async Task<ActionResult> GetSessionsForProfile(int profileID) {
        return ObserveDataOperationResult(
            await data.GetSessionsForProfile(profileID)
        );
    }

    // GET api/monitorSessions/1/report
    [HttpGet("{sessionID:int}/report")]
    public async Task<ActionResult> GetSessionReport(int sessionID) {
        return ObserveDataOperationResult(
            await data.GetSessionReport(sessionID)
        );
    }

    // POST api/monitorSessions/newFromProfile/1
    [HttpPost("newFromProfile/{profileID:int}")]
    public async Task<ActionResult> GetNewSessions(int profileID) {
        return ObserveDataOperationResult(
            await data.GetNewSession(profileID)
        );
    }

    // POST api/monitorSessions/1/addPulse
    [HttpPost("{sessionID:int}/addPulse")]
    public async Task<ActionResult> SavePulseResult(
        int sessionID,
        [FromBody] MonitoringPulseResult pulseResult
    ) {
        return ObserveDataOperationResult(
            await data.SavePulseResult(sessionID, pulseResult, pulseResult.Messages)
        );
    }
}

}
