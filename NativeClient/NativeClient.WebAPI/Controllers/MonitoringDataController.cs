using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;

using Data.Model.ViewModel;
using Data.Abstract.DataAccessServices;

namespace NativeClient.WebAPI.Controllers {

[Route("api/monitoring")]
public class MonitoringDataController : BaseDataController {
    readonly IMonitoringDataService data;

    MonitoringDataController(IMonitoringDataService _data) {
        data = _data;
    }

    // GET api/monitoring/sessionsForProfile/1
    [HttpGet("/sessionsForProfile/{profileID:int}")]
    public async Task<ActionResult> GetSessionsForProfile(int profileID) {
        return await GetDbData(async () =>
            await data.GetSessionsForProfile(profileID)
        );
    }

    // POST api/monitoring/newSession
    [HttpPost("/newSession")]
    public async Task<ActionResult> GetNewSessions(int profileID) {
        return await GetDbData(async () =>
            await data.GetNewSession(profileID)
        );
    }

    // POST api/monitoring/newSession
    [HttpPost("/savePulse/{sessionID:int}")]
    public async Task<ActionResult> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult
    ) {
        return await GetDbData(async () =>
            await data.SavePulseResult(sessionID, pulseResult, pulseResult.Messages)
        );
    }
}

}
