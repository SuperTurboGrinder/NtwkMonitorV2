import { Injectable } from '@angular/core';
import { MonitoringSession } from '../model/httpModel/monitoringSession.model';
import { MonitoringPulseResult } from '../model/httpModel/monitoringPulseResult.model';
import { MonitoringDataService } from './monitoringData.service';
import { Observable, of } from 'rxjs';
import { switchMap, take } from 'rxjs/operators';

@Injectable()
export class CurrentMonitoringSessionDataService {
    private currentSession: MonitoringSession = null;
    private currentSessionPulses: MonitoringPulseResult[] = [];

    constructor(
        private monitoringDataService: MonitoringDataService
    ) { }

    public get session(): MonitoringSession {
        return this.currentSession;
    }

    public get pulses(): MonitoringPulseResult[] {
        return this.currentSessionPulses;
    }

    public createNewSession(
        settingsProfileID: number,
        onSuccess: (sessionID: number) => void
    ) {
        this.currentSession = null;
        this.currentSessionPulses = [];
        this.monitoringDataService.getNewSession(settingsProfileID)
            .pipe(
                switchMap(sessionResult => {
                    if (sessionResult.success === true) {
                        this.currentSession = sessionResult.data;
                        return of(this.currentSession.id);
                    }
                    return of(<number>null);
                }),
                take(1)
            )
            .subscribe(sessionID => {
                if (sessionID !== null) {
                    onSuccess(sessionID);
                }
            });
    }

    public addNewPulse(newPulse: MonitoringPulseResult) {
        if (this.currentSession !== null) {
            const sessionID = this.currentSession.id;
            this.monitoringDataService.saveSessionPulse(
                sessionID,
                newPulse
            ).subscribe(savedPulse => {
                if (savedPulse.success === true
                && this.currentSession.id === sessionID) {
                    MonitoringPulseResult.convertJSTime(savedPulse.data);
                    this.currentSessionPulses.push(savedPulse.data);
                }
            });
        }
    }
}
