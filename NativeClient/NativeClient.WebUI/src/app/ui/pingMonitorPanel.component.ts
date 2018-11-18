import { Component, OnDestroy } from '@angular/core';
import { PingMonitorService } from '../services/pingMonitor.service';
import { CurrentMonitoringSessionDataService } from '../services/currentMonitorSessionData.service';
import { MonitoringSession } from '../model/httpModel/monitoringSession.model';
import { MonitoringPulseResult } from '../model/httpModel/monitoringPulseResult.model';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-ping-monitor-panel',
    templateUrl: './pingMonitorPanel.component.html'
})
export class PingMonitorPanelComponent implements OnDestroy {
    public isOpened = false;
    public currentSessionData: {
        session: MonitoringSession,
        pulses: MonitoringPulseResult[]
    };
    private currentDataSubscription: Subscription = null;

    public ngOnDestroy() {
        this.currentDataSubscription.unsubscribe();
    }

    constructor(
        private pingMonitorService: PingMonitorService,
        private currentMonitorDataService: CurrentMonitoringSessionDataService
    ) {
        this.currentDataSubscription = this.currentMonitorDataService
            .subscribeToSessionData(data => this.currentSessionData = data);
    }

    public get isActive(): boolean {
        return this.pingMonitorService.isActive;
    }

    public get isPulsing(): boolean {
        return this.pingMonitorService.isPulsing;
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
