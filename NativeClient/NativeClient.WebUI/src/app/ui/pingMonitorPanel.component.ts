import { Component } from '@angular/core';
import { PingMonitorService } from '../services/pingMonitor.service';
import { CurrentMonitoringSessionDataService } from '../services/currentMonitorSessionData.service';
import { MonitoringSession } from '../model/httpModel/monitoringSession.model';
import { MonitoringPulseResult } from '../model/httpModel/monitoringPulseResult.model';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-ping-monitor-panel',
    templateUrl: './pingMonitorPanel.component.html'
})
export class PingMonitorPanelComponent {
    public isOpened = false;
    public currentSessionData: {
        session: MonitoringSession,
        pulses: MonitoringPulseResult[]
    };
    private currentDataSubscription: Subscription = null;

    constructor(
        private pingMonitorService: PingMonitorService,
        private currentMonitorDataService: CurrentMonitoringSessionDataService
    ) {
        this.currentDataSubscription = this.currentMonitorDataService
            .subscribeToSessionData(data => this.currentSessionData = data);
    }

    public get isActive() {
        return this.pingMonitorService.isActive;
    }

    public get sessionData(): {
        session: MonitoringSession,
        pulses: MonitoringPulseResult[]
    } {
        return this.currentSessionData;
    }

    public openCloseSwitch() {
        if (this.pingMonitorService.isActive === false) {
            this.isOpened = false;
        } else {
            this.isOpened = !this.isOpened;
        }
    }
}
