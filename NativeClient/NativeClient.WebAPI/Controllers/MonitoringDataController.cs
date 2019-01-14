using System.Threading.Tasks;
using Data.Abstract.DataAccessServices;
using Data.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using NativeClient.WebAPI.Abstract;

namespace NativeClient.WebAPI.Controllers
{
    [Route("api/monitorSessions")]
    public class MonitoringDataController : BaseDataController
    {
        readonly IMonitoringDataService _data;

        public MonitoringDataController(
            IMonitoringDataService data,
            IErrorReportAssemblerService errAssembler
        ) : base(errAssembler)
        {
            _data = data;
        }

        // GET api/monitorSessions/forProfile/1
        [HttpGet("forProfile/{profileID:int}")]
        public async Task<ActionResult> GetSessionsForProfile(int profileId)
        {
            return ObserveDataOperationResult(
                await _data.GetSessionsForProfile(profileId)
            );
        }

        // GET api/monitorSessions/1/report
        [HttpGet("{sessionID:int}/report")]
        public async Task<ActionResult> GetSessionReport(int sessionId)
        {
            return ObserveDataOperationResult(
                await _data.GetSessionReport(sessionId)
            );
        }

        // POST api/monitorSessions/newFromProfile/1
        [HttpPost("newFromProfile/{profileID:int}")]
        public async Task<ActionResult> GetNewSessions(int profileId)
        {
            return ObserveDataOperationResult(
                await _data.GetNewSession(profileId)
            );
        }

        // POST api/monitorSessions/1/addPulse
        [HttpPost("{sessionID:int}/addPulse")]
        public async Task<ActionResult> SavePulseResult(
            int sessionId,
            [FromBody] MonitoringPulseResult pulseResult
        )
        {
            return ObserveDataOperationResult(
                await _data.SavePulseResult(sessionId, pulseResult, pulseResult.Messages)
            );
        }
    }
}