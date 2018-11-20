import { Injectable } from '@angular/core';
import { MonitoringSession } from '../model/httpModel/monitoringSession.model';
import { MonitoringPulseResult } from '../model/httpModel/monitoringPulseResult.model';
import { MonitoringDataService } from './monitoringData.service';
import { Observable, of, Subscription, BehaviorSubject } from 'rxjs';
import { switchMap, take } from 'rxjs/operators';

@Injectable()
export class CurrentMonitoringSessionDataService {
    private currentSession: MonitoringSession = null;
    private currentSessionPulses: MonitoringPulseResult[] = [];
    private dataSubject = new BehaviorSubject<{
        session: MonitoringSession,
        pulses: MonitoringPulseResult[]
    }>({ session: null, pulses: [] });

    constructor(
        private monitoringDataService: MonitoringDataService
    ) { }

    public subscribeToSessionData(callback: (data: {
        session: MonitoringSession,
        pulses: MonitoringPulseResult[]
    }) => void): Subscription {
        return this.dataSubject.asObservable().subscribe(
            callback
        );
    }

    private sendData() {
        this.dataSubject.next({
            session: this.currentSession,
            pulses: this.currentSessionPulses
        });
    }

    public clearData() {
        this.dataSubject.next({
            session: null,
            pulses: []
        });
    }

    public createNewSession(
        settingsProfileID: number,
        onSuccess: () => void
    ) {
        this.currentSession = null;
        this.currentSessionPulses = [];
        this.monitoringDataService.getNewSession(settingsProfileID)
            .pipe(
                switchMap(sessionResult => {
                    if (sessionResult.success === true) {
                        this.currentSession = sessionResult.data;
                        this.sendData();
                    }
                    return of(sessionResult.success);
                }),
                take(1)
            )
            .subscribe(success => {
                if (success === true) {
                    onSuccess();
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
                    this.sendData();
                }
            });
        }
    }
}
