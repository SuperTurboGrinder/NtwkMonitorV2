import { Injectable, Inject } from '@angular/core';
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model';
import { BaseURL } from './baseUrl.token';
import { Observable } from 'rxjs';
import { MonitoringSession } from '../model/httpModel/monitoringSession.model';
import { MonitoringPulseResult } from '../model/httpModel/monitoringPulseResult.model';

@Injectable()
export class MonitoringDataService {
    private baseUrl: string = null;

    constructor(
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'monitorSessions';
    }

    public getSessionsForProfile(
        profileID: number
    ): Observable<HTTPResult<MonitoringSession[]>> {
        return this.httpDatasource.dataRequest<MonitoringSession[]>(
            'get',
            this.baseUrl + `/forProfile/${profileID}`
        );
    }

    public getSessionReport(
        sessionID: number
    ): Observable<HTTPResult<MonitoringPulseResult[]>> {
        return this.httpDatasource.dataRequest<MonitoringPulseResult[]>(
            'get',
            this.baseUrl + `/${sessionID}/report`
        );
    }

    public getNewSession(
        profileID: number
    ): Observable<HTTPResult<MonitoringSession>> {
        return this.httpDatasource.dataRequest<MonitoringSession>(
            'post',
            this.baseUrl + `/newFromProfile/${profileID}`
        );
    }

    public saveSessionPulse(
        sessionID: number,
        pulse: MonitoringPulseResult
    ): Observable<HTTPResult<MonitoringPulseResult>> {
        return this.httpDatasource.dataRequest<MonitoringPulseResult>(
            'post',
            this.baseUrl + `/${sessionID}/addPulse`,
            pulse
        );
    }
}
