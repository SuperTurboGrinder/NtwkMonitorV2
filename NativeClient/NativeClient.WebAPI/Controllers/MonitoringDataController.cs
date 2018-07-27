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
    [HttpGet("/sessionsForProfile/{profileID}")]
    public async Task<ActionResult> GetSessionsForProfile(int profileID) {
        return await GetDbData(async () =>
            await data.GetSessionsForProfile(profileID)
        );
    }

    // GET api/monitoring/sessionReoprts/1
    [HttpPost("/sessionReports/{sessionID}")]
    public async Task<ActionResult> GetSessionReport(int sessionID) {
        return await GetDbData(async () =>
            await data.GetSessionReport(sessionID)
        );
    }

    // GET api/monitoring/5
    [HttpGet("{id}")]
    public string Get(int id) {
        return "value";
    }

    // POST api/monitoring
    [HttpPost]
    public void Post([FromBody]string value) {
    }

    // PUT api/monitoring/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody]string value) {
    }

    // DELETE api/monitoring/5
    [HttpDelete("{id}")]
    public void Delete(int id) {
    }
}

}
