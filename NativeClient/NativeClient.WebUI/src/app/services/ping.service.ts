import { Injectable, Inject } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';

import { PingTestData } from '../model/httpModel/pingTestData.model';
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model';
import { BaseURL } from './baseUrl.token';

@Injectable()
export class PingService {
    private baseUrl: string = null;

    constructor(
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'services';
    }

    getPing(nodeID: number): Observable<HTTPResult<PingTestData>> {
        return this.httpDatasource.dataRequest<PingTestData>(
            'get',
            this.baseUrl + `/ping/${nodeID}`
        );
    }

    // should not use this one with ui
    // multiple failed pings can be very slow (4sec per node)
    // would look like it is not working
    getListPing(nodesIDs: number[]): Observable<HTTPResult<PingTestData[]>> {
        return this.httpDatasource.dataRequest<number[], PingTestData[]>(
            'post',
            this.baseUrl + `/pingList`,
            nodesIDs
        );
    }
}
