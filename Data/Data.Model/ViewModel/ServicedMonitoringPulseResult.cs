using System;
using System.Collections.Generic;

namespace Data.Model.ViewModel {

public class MonitoringPulseResult {
    public int Responded;
    public int Silent;
    public int Skipped;
    public DateTime CreationTime;
    public IEnumerable<MonitoringMessage> Messages;
}

}