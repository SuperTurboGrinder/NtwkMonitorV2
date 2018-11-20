import { Component, OnDestroy } from '@angular/core';
import { MonitoringDataService } from '../services/monitoringData.service';
import { SettingsProfilesService } from '../services/settingsProfiles.service';
import { MonitoringSession } from '../model/httpModel/monitoringSession.model';
import { MonitoringPulseResult } from '../model/httpModel/monitoringPulseResult.model';
import { Subscription } from 'rxjs';
import { CurrentMonitoringSessionDataService } from '../services/currentMonitorSessionData.service';

@Component({
    selector: 'app-history-view',
    templateUrl: './historyView.component.html'
})
export class HistoryViewComponent implements OnDestroy {
    private currentMonitoringSessionID: number = null;
    private currentSettingsProfileID: number = null;
    private unfilteredSessions: MonitoringSession[] = null;
    public sessions: MonitoringSession[] = null;
    public selectedSessionData: {
        session: MonitoringSession,
        pulses: MonitoringPulseResult[]
    } = { session: null, pulses: [] };
    private settingsSubscription: Subscription = null;
    private monitorDataSubscription: Subscription = null;

    public isLoadingError = false;
    public loadingReport = false;

    constructor(
        private monitoringDataSerivce: MonitoringDataService,
        settingsService: SettingsProfilesService,
        pingMonitorCurrentData: CurrentMonitoringSessionDataService
    ) {
        this.monitorDataSubscription = pingMonitorCurrentData
            .subscribeToSessionData(data => {
                if (data.session !== null) {
                    this.currentMonitoringSessionID = data.session.id;
                } else {
                    this.currentMonitoringSessionID = null;
                }
                this.refilterSessions();
            });
            this.settingsSubscription = settingsService
                .subscribeToSettingsChange(settings =>
                    this.loadSessions(settings.profile.id)
                );
    }

    ngOnDestroy() {
        if (this.settingsSubscription !== null) {
            this.settingsSubscription.unsubscribe();
        }
        if (this.monitorDataSubscription) {
            this.monitorDataSubscription.unsubscribe();
        }
    }

    private refilterSessions() {
        if (this.unfilteredSessions === null) {
            return;
        }
        if (this.currentMonitoringSessionID === null) {
            this.sessions = this.unfilteredSessions;
        } else {
            this.sessions = this.unfilteredSessions.filter(s =>
                s.id !== this.currentMonitoringSessionID
            );
        }
    }

    private loadSessions(settingsProfileID: number) {
        this.currentSettingsProfileID = settingsProfileID;
        this.sessions = null;
        this.monitoringDataSerivce.getSessionsForProfile(settingsProfileID)
            .subscribe(sessionsResult => {
                if (sessionsResult.success === true) {
                    this.unfilteredSessions = sessionsResult.data
                        .map(s => {
                            MonitoringSession.convertJSTime(s);
                            return s;
                        });
                    this.refilterSessions();
                    this.isLoadingError = false;
                }
                this.isLoadingError = true;
            });
    }

    refresh(_: boolean) {
        this.loadSessions(this.currentSettingsProfileID);
    }

    public formatDate(session: MonitoringSession): string {
        const date = session.creationTimeJS.toLocaleDateString();
        const time = session.creationTimeJS.toLocaleTimeString();
        const endTime = session.lastPulseTimeJS.toLocaleTimeString();
        return `(${date})  ${time} - ${endTime}`;
    }

    public isSelectedSession(session: MonitoringSession): boolean {
        return this.selectedSessionData.session !== null
        && this.selectedSessionData.session.id === session.id;
    }

    public selectSession(session: MonitoringSession) {
        this.loadingReport = true;
        this.monitoringDataSerivce.getSessionReport(session.id)
            .subscribe(reportResult => {
                if (reportResult.success === true) {
                    this.selectedSessionData = ({
                        session: session,
                        pulses: reportResult.data.map(p => {
                            MonitoringPulseResult.convertJSTime(p);
                            return p;
                        })
                    });
                }
                this.loadingReport = false;
            });
    }
}
