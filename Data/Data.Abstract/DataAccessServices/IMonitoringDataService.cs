using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

//input data validation
//model convertion
//reporting errors through IDataActionResult
public interface IMonitoringDataService {
    
    Task<IDataActionResult<MonitoringSession>> GetNewSession(
        int profileID,
        IEnumerable<int> monitoredTagsIDList
    );
    Task<IDataActionResult<MonitoringPulseResult>> SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult,
        IEnumerable<MonitoringMessage> messages
    );
    Task<IDataActionVoidResult> ClearEmptySessions();

    Task<IDataActionResult<IEnumerable<MonitoringSession>>> GetSessionsForProfile(int profileID);
    //includes messages
    Task<IDataActionResult<IEnumerable<MonitoringPulseResult>>> GetSessionReport(int monitoringSessionID);

}

}