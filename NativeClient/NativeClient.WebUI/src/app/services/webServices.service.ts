import { Injectable, Inject } from '@angular/core';
import { Observable, Observer, Subject } from 'rxjs';

import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model'
import { BaseURL } from "./baseUrl.token";

@Injectable()
export class WebServicesService {
    private baseUrl: string = null;

    constructor(
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'services/customWebService';
    }

    runCustomWebService(nodeID: number, webServiceID: number) {
        this.httpDatasource.operationRequest(
            "get",
            this.baseUrl+`/${nodeID}/${webServiceID}`
        ).subscribe(success => {
            if(success === true) {
                //send message about successful web service launch
            } else {
                //send message about failed web service launch
            }
        })
    }
}