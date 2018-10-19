import { Injectable, Inject } from '@angular/core';

import { HTTPDatasource } from './http.datasource';
import { BaseURL } from './baseUrl.token';

@Injectable()
export class ExecServicesService {
    private baseUrl: string = null;

    constructor(
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'services';
    }

    runTelnetService(nodeID: number) {
        this.httpDatasource.operationRequest(
            'get',
            this.baseUrl + `/telnet/${nodeID}`
        ).subscribe(success => {
            if (success === true) {
                // send message about successful telnet launch
            } else {
                // send message about failed telnet launch
            }
        });
    }

    runSSHService(nodeID: number) {
        this.httpDatasource.operationRequest(
            'get',
            this.baseUrl + `/ssh/${nodeID}`
        ).subscribe(success => {
            if (success === true) {
                // send message about successful ssh launch
            } else {
                // send message about failed ssh launch
            }
        });
    }
}
