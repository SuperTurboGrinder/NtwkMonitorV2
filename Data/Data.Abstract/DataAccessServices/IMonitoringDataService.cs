using System.Collections.Generic;
using System;

using Data.Model.ViewModel;

namespace Data.Abstract.DataAccessServices {

public interface IMonitoringDataService {
    IDataActionResult<MonitoringSession> GetNewSession(int tagID);
    IDataActionVoidResult SavePulseResult(
        int sessionID,
        MonitoringPulseResult pulseResult
    );

    IDataActionResult<IEnumerable<MonitoringSession>> GetSessionsFor(
        int networkID);
    IDataActionResult<IEnumerable<MonitoringPulseResult>> GetPulsesFor(
        int monitoringSessionID);
}

}