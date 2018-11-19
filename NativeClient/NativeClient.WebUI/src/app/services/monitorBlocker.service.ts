import { PingMonitorService } from './pingMonitor.service';

export class MonitorBlockerService {
    private hideMonitorPanel = false;

    public get isMonitorPanelHidden() {
        return this.hideMonitorPanel;
    }

    public hideMonitor() {
        this.hideMonitorPanel = true;
    }

    public showMonitor() {
        this.hideMonitorPanel = false;
    }
}
