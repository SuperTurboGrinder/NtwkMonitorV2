using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through IDataActionResult
public interface IMonitoringDataService {
    
    Task<DataActionResult<MonitoringSession>> GetNewSession(
        int profileID,
        IEnumerable<int> monitoredTagsIDList
    );
    Task<DataActionResult<MonitoringPulseResult>> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    );
    Task<DataActionVoidResult> ClearEmptySessions();

    Task<DataActionResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileID);
    //includes messages
    Task<DataActionResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionID);

}

}