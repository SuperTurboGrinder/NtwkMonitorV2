import { Component, OnDestroy } from '@angular/core';
import { PingMonitorService } from '../services/pingMonitor.service';
import { CurrentMonitoringSessionDataService } from '../services/currentMonitorSessionData.service';
import { MonitoringSession } from '../model/httpModel/monitoringSession.model';
import { MonitoringPulseResult } from '../model/httpModel/monitoringPulseResult.model';
import { Subscription, Observable, timer } from 'rxjs';
import { MonitorBlockerService } from '../services/monitorBlocker.service';

@Component({
    selector: 'app-ping-monitor-panel',
    templateUrl: './pingMonitorPanel.component.html'
})
export class PingMonitorPanelComponent implements OnDestroy {
    static readonly emptyPulse = new MonitoringPulseResult(
        0, 0, 0, 0, []
    );
    static readonly noTime = '--';
    private pulseTimeCache: {
        seconds: number,
        val: string
    } = ({seconds: 0, val: '--'});
    public isOpened = false;
    public currentSessionData: {
        session: MonitoringSession,
        pulses: MonitoringPulseResult[]
    };
    private _displaySessionEndMessage = false;
    private currentDataSubscription: Subscription = null;
    private timerSubscription: Subscription = null;

    public ngOnDestroy() {
        if (this.currentDataSubscription !== null) {
            this.currentDataSubscription.unsubscribe();
        }
        if (this.timerSubscription !== null) {
            this.timerSubscription.unsubscribe();
        }
    }

    constructor(
        private monitorBlocker: MonitorBlockerService,
        private pingMonitorService: PingMonitorService,
        private currentMonitorDataService: CurrentMonitoringSessionDataService
    ) {
        this.currentDataSubscription = this.currentMonitorDataService
            .subscribeToSessionData(data => this.currentSessionData = data);
        this.timerSubscription = timer(1000, 1000).subscribe(
            _ => this.updateTimeToNextPulse()
        );
    }

    public get displaySessionEndMessage(): boolean {
        return this._displaySessionEndMessage;
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

    public monitorSwitch() {
        if (this.pingMonitorService.isActive) {
            this.tryEndSession();
        } else {
            this.pingMonitorService.startMonitor();
        }
    }

    public tryEndSession() {
        this._displaySessionEndMessage = true;
    }

    public endSession(shouldEnd: boolean) {
        if (shouldEnd === true) {
            this.pingMonitorService.stopMonitor();
            this.isOpened = false;
            this.currentMonitorDataService.clearData();
        }
        this._displaySessionEndMessage = false;
    }

    private updateTimeToNextPulse() {
        const toNextPulse = this.pingMonitorService.toNextPulse;
        if (toNextPulse === null || toNextPulse < 0) {
            this.pulseTimeCache.seconds = 0;
            this.pulseTimeCache.val = PingMonitorPanelComponent.noTime;
        } else {
            const secondsToPulse = Math.floor(toNextPulse / 1000);
            if (this.pulseTimeCache.seconds !== secondsToPulse) {
                this.pulseTimeCache.seconds = secondsToPulse;
                const minutesToPulse = Math.floor(secondsToPulse / 60);
                this.pulseTimeCache.val = `${minutesToPulse}:${secondsToPulse - (minutesToPulse * 60)}`;
            }
        }
    }

    public timeToNextPulse(): string {
        return this.pulseTimeCache.val;
    }

    public get lastPulse() {
        return this.isActive === false
        || this.sessionData.pulses.length === 0
            ? PingMonitorPanelComponent.emptyPulse
            : this.sessionData.pulses[this.sessionData.pulses.length - 1];
    }

    public get shouldHide(): boolean {
        return this.pingMonitorService.isReady === false
        || (this.isActive !== true
        && this.monitorBlocker.isMonitorPanelHidden === true);
    }
}
