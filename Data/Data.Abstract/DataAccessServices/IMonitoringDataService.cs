using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Model.ViewModel;
using Data.Model.ResultsModel;

namespace Data.Abstract.DataAccessServices
{
//validation -> conversion -> DataActionResult
    public interface IMonitoringDataService
    {
        Task<DataActionResult<MonitoringSession>> GetNewSession(int profileId);

        Task<DataActionResult<MonitoringPulseResult>> SavePulseResult(
            int sessionId,
            MonitoringPulseResult pulseResult,
            IEnumerable<MonitoringMessage> messages
        );

        Task<StatusMessage> ClearEmptySessions();

        Task<DataActionResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileId);

        //includes messages
        Task<DataActionResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionId);
    }
}