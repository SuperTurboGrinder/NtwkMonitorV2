import { Component } from '@angular/core';
import { PingMonitorService } from '../services/pingMonitor.service';
import { CurrentMonitoringSessionDataService } from '../services/currentMonitorSessionData.service';
import { MonitoringSession } from '../model/httpModel/monitoringSession.model';
import { MonitoringPulseResult } from '../model/httpModel/monitoringPulseResult.model';

@Component({
    selector: 'app-ping-monitor-panel',
    templateUrl: './pingMonitorPanel.component.html'
})
export class PingMonitorPanelComponent {
    public isOpened = false;

    constructor(
        private pingMonitorService: PingMonitorService,
        private currentMonitorDataService: CurrentMonitoringSessionDataService
    ) {}

    public get isActive() {
        return this.pingMonitorService.isActive;
    }

    public get currentSession(): MonitoringSession {
        return this.currentMonitorDataService.session;
    }

    public get currentSessionPulses(): MonitoringPulseResult[] {
        return this.currentMonitorDataService.pulses;
    }

    public openCloseSwitch() {
        if (this.pingMonitorService.isActive === false) {
            this.isOpened = false;
        } else {
            this.isOpened = !this.isOpened;
        }
    }
}
