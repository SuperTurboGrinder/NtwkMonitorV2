import { Injectable, Inject } from "@angular/core";
import { Observable, BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";

import { PingTestData } from "../model/httpModel/pingTestData.model";
import { HTTPDatasource } from './http.datasource';
import { HTTPResult } from '../model/servicesModel/httpResult.model'
import { UpdateAlarmService } from './updateAlarm.service';
import { BaseURL } from "./baseUrl.token";
import { HttpRequest } from "@angular/common/http";

@Injectable()
export class PingService {
    private baseUrl: string = null;

    constructor(
        private httpDatasource: HTTPDatasource,
        @Inject(BaseURL) _baseUrl: string
    ) {
        this.baseUrl = _baseUrl + 'services/ping';
    }

    getPing(nodeID: number) : Observable<HTTPResult<PingTestData>> {
        return this.httpDatasource.dataRequest<PingTestData>(
            "get",
            this.baseUrl+`/${nodeID}`
        )
    }
}